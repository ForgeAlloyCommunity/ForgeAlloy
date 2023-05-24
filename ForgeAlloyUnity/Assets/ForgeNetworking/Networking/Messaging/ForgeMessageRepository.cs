using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Forge.Factory;
using Forge.Utilities;

namespace Forge.Networking.Messaging
{
	public class ForgeMessageRepository : IMessageRepository
	{
		private CancellationTokenSource _ttlBackgroundToken;

		private struct StoredTTLMessage
		{
			public DateTime ttl;
			public IMessage message;
			public EndPoint receiver;
			public long IPKey;
		}
		private struct StoredMessage
		{
			public DateTime TimeSent;
			public DateTime TimeResend;
			public IMessage message;
		}

		private readonly List<StoredTTLMessage> _messagesWithTTL = new List<StoredTTLMessage>();
		private readonly Dictionary<long, Dictionary<IMessageReceiptSignature, StoredMessage>> _messages = new Dictionary<long, Dictionary<IMessageReceiptSignature, StoredMessage>>();
		private readonly Dictionary<long, EndPoint> _endpoints = new Dictionary<long, EndPoint>();
		private Dictionary<long, IForgeEndpointRepeater> _endpointRepeater = new Dictionary<long, IForgeEndpointRepeater>();
		private double ResendPingMultiplier = 1.1;
		private double ResendMin = 50;
		public string Id { get; set; }

		public void Clear()
		{
			_ttlBackgroundToken.Cancel();
			lock (_messagesWithTTL)
			{
				_messagesWithTTL.Clear();
			}
			lock (_messages)
			{
				_messages.Clear();
				_endpoints.Clear();
			}
		}

		private void TTLBackground()
		{
			try
			{
				while (true)
				{
					_ttlBackgroundToken.Token.ThrowIfCancellationRequested();
					var now = DateTime.UtcNow;
					int expiredId = 0;
					int i = 0;

					while (expiredId > -1 && _messagesWithTTL.Count > 0)
					{
						expiredId = -1;

						// Search for an expired message
						lock (_messagesWithTTL)
						{
							for (; i < _messagesWithTTL.Count; i++)
								if (_messagesWithTTL[i].ttl <= now)
								{ expiredId = i; break; }
						}

						if (expiredId > -1)
						{
							//TODO: Raise an event if a message timed out. Return IMessage in the even
							RemoveFromMessageLookup(_messagesWithTTL[expiredId].receiver, _messagesWithTTL[expiredId].message.Receipt, false);
						}
					}

					if (_messagesWithTTL.Count == 0)
						break;

					Thread.Sleep(10);
				}
			}
			catch (OperationCanceledException) { }
		}


		public void AddMessage(IMessage message, EndPoint receiver)
		{
			//message.Receipt = GetNewMessageReceipt(receiver);

			if (message.Receipt == null)
				throw new MessageRepositoryMissingReceiptOnMessageException();
			if (Exists(receiver, message.Receipt))
				throw new MessageWithReceiptSignatureAlreadyExistsException();

			lock (_messages)
			{
				if (!_messages.TryGetValue(receiver.IPKey(), out var kv))
				{
					kv = new Dictionary<IMessageReceiptSignature, StoredMessage>();
					_messages.Add(receiver.IPKey(), kv);
					_endpoints.Add(receiver.IPKey(), receiver);
				}
				if (!_endpointRepeater.ContainsKey(receiver.IPKey()))
				{
					_endpointRepeater.Add(receiver.IPKey(), AbstractFactory.Get<INetworkTypeFactory>().GetNew<IForgeEndpointRepeater>());
				}
				message.IsBuffered = true;
				double resendDelay = Math.Max((double)_endpointRepeater[receiver.IPKey()].Ping * ResendPingMultiplier, ResendMin);
				kv.Add(message.Receipt, new StoredMessage
				{
					TimeSent = DateTime.UtcNow,
					TimeResend = DateTime.UtcNow.AddMilliseconds(resendDelay),
					message = message
				});
			}
		}

		public void AddMessage(IMessage message, EndPoint receiver, int ttlMilliseconds)
		{
			AddMessage(message, receiver);

			var span = new TimeSpan(0, 0, 0, 0, ttlMilliseconds <= 0 ? GlobalConst.maxMessageRepeatMilliseconds : ttlMilliseconds);
			var now = DateTime.UtcNow;
			lock (_messagesWithTTL)
			{
				_messagesWithTTL.Add(new StoredTTLMessage
				{
					ttl = now + span,
					message = message,
					receiver = receiver
				});
				if (_messagesWithTTL.Count == 1)
				{
					_ttlBackgroundToken = new CancellationTokenSource();
					Task.Run(TTLBackground, _ttlBackgroundToken.Token);
				}
			}
		}

		public void RemoveAllFor(EndPoint sender)
		{
			var copy = new List<IMessage>();

			lock (_messages)
			{
				var removals = new List<IMessageReceiptSignature>();
				if (_messages.TryGetValue(sender.IPKey(), out var kv))
				{
					foreach (var mkv in kv)
						copy.Add(mkv.Value.message);
				}
				_messages.Remove(sender.IPKey());
				_endpoints.Remove(sender.IPKey());
			}

			try
			{
				foreach (var m in copy)
					m.Unbuffered();
				_endpointRepeater.Remove(sender.IPKey());
			}
			catch { }
		}

		public void RemoveMessage(EndPoint sender, IMessage message)
		{
			RemoveMessage(sender, message.Receipt);
		}

		public void RemoveMessage(EndPoint sender, IMessageReceiptSignature sig)
		{
			RemoveFromMessageLookup(sender, sig);
		}


		public void RemoveMessage(EndPoint sender, IMessageReceiptSignature receipt, ushort recentPackets)
		{
			RemoveFromMessageLookup(sender, receipt);

			// Remove recent messages if in buffer
			for (int i = 1; i <= 16; ++i)
			{
				if ((recentPackets & 1) != 0)
				{
					int hash = (receipt.GetHashCode() - i);
					RemoveFromMessageLookup(sender, hash);
				}
				recentPackets >>= 1;
			}
		}

		private void RemoveFromMessageLookup(EndPoint sender, IMessageReceiptSignature receipt, bool updatePing = true)
		{

			lock (_messages)
			{
				if (_messages.TryGetValue(sender.IPKey(), out var kv))
				{
					try
					{
						if (kv.ContainsKey(receipt))
						{
							if (updatePing)
								_endpointRepeater[sender.IPKey()]?.AddPing(
									(int)((DateTime.UtcNow - kv[receipt].TimeSent).TotalMilliseconds));
							kv[receipt].message.Unbuffered();
							kv.Remove(receipt);
						}
					}
					catch {	} // Catch just in case message already removed
				}
				else
					Console.WriteLine($"Remove message failed because sender not found {sender}");
			}

			RemoveFromTTL(sender, receipt);

		}

		private void RemoveFromMessageLookup(EndPoint sender, int hash)
		{
			lock (_messages)
			{
				if (_messages.TryGetValue(sender.IPKey(), out var kv))
				{
					try
					{
						IMessageReceiptSignature sig = null;
						foreach (var m in kv)
						{
							if (m.Key.GetHashCode() == hash)
							{
								sig = m.Key;
								break;
							}
						}
						if (sig != null)
						{
							RemoveFromMessageLookup(sender, sig, false);
						}
					}
					catch { } // just in case
				}
			}
		}


		public bool Exists(EndPoint sender, IMessageReceiptSignature receipt)
		{
			bool exists = false;
			lock (_messages)
			{
				if (_messages.TryGetValue(sender.IPKey(), out var kv))
					exists = kv.ContainsKey(receipt);
			}
			return exists;
		}

		private void RemoveFromTTL(EndPoint sender, IMessageReceiptSignature receipt)
		{
			lock (_messagesWithTTL)
			{
				for (int i = 0; i < _messagesWithTTL.Count; i++)
				{
					if (_messagesWithTTL[i].message.Receipt != null) // find out why this happens
					{
						if (_messagesWithTTL[i].message.Receipt.Equals(receipt) && _messagesWithTTL[i].receiver.Equals(sender))
						{
							_messagesWithTTL.RemoveAt(i);
							break;
						}
					}
				}
			}
		}

		public double GetTTLDiff(EndPoint sender, IMessageReceiptSignature receipt, int ttlMilliseconds)
		{
			double diff = -1;
			lock (_messagesWithTTL)
			{
				for (int i = 0; i < _messagesWithTTL.Count; i++)
				{
					try
					{
						if (_messagesWithTTL[i].message.Receipt != null)  // Not sure why this happens
						{
							if (_messagesWithTTL[i].message.Receipt.Equals(receipt))
							{
								var span = new TimeSpan(0, 0, 0, 0, ttlMilliseconds);
								diff = (DateTime.UtcNow - (_messagesWithTTL[i].ttl - span)).TotalMilliseconds;
								break;
							}
						}
					}
					catch (Exception ex)
					{
						//UnityEngine.Debug.LogError($"GetTTLDiff: {receipt.ToString()}: {ex.Message}");
					}
				}

			}
			return diff;
		}

		public void Iterate(MessageRepositoryIterator iterator)
		{
			// TODO:  Review this for better performance
			var copy = new List<KeyValuePair<EndPoint, IMessage>>();
			lock (_messages)
			{
				foreach (var kv in _messages)
				{
					foreach (var mkv in kv.Value)
					{
						if (mkv.Value.TimeResend < DateTime.UtcNow)
						{
							copy.Add(new KeyValuePair<EndPoint, IMessage>(_endpoints[kv.Key], mkv.Value.message));
						}
					}
				}
			}
			foreach (var kv in copy)
			{
				try
				{
					iterator(kv.Key, kv.Value);
				}
				catch { } // Ignore error when client disconnects
			}
		}

		public int GetMessageCount()
		{
			return _messages.Count;
		}

		public ushort ProcessReliableSignature(EndPoint sender, int id)
		{
			if (!_endpointRepeater.ContainsKey(sender.IPKey()))
			{
				_endpointRepeater.Add(sender.IPKey(), AbstractFactory.Get<INetworkTypeFactory>().GetNew<IForgeEndpointRepeater>());
			}
			return _endpointRepeater[sender.IPKey()].ProcessReliableSignature(id);
		}

		public IMessageReceiptSignature GetNewMessageReceipt(EndPoint receiver)
		{
			if (!_endpointRepeater.ContainsKey(receiver.IPKey()))
			{
				_endpointRepeater.Add(receiver.IPKey(), AbstractFactory.Get<INetworkTypeFactory>().GetNew<IForgeEndpointRepeater>());
			}
			return _endpointRepeater[receiver.IPKey()].GetNewMessageReceipt();
		}
	}
}
