using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Forge.Factory;

namespace Forge.Networking.Messaging
{
	public class ForgeMessageRepository : IMessageRepository
	{
		private CancellationTokenSource _ttlBackgroundToken;

		private struct StoredTTLMessage
		{
			public DateTime ttl;
			public IMessage message;
			public EndPoint sender;
		}
		private struct StoredMessage
		{
			public DateTime TimeSent;
			public DateTime TimeResend;
			public IMessage message;
		}

		private readonly List<StoredTTLMessage> _messagesWithTTL = new List<StoredTTLMessage>();
		private readonly Dictionary<EndPoint, Dictionary<IMessageReceiptSignature, StoredMessage>> _messages = new Dictionary<EndPoint, Dictionary<IMessageReceiptSignature, StoredMessage>>();
		private Dictionary<EndPoint, IForgeEndpointRepeater> _endpointRepeater = new Dictionary<EndPoint, IForgeEndpointRepeater>();
		private double ResendPingMultiplier = 1.1;
		private double ResendMin = 50;
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
					lock (_messagesWithTTL)
					{
						for (int i = 0; i < _messagesWithTTL.Count; i++)
						{
							if (_messagesWithTTL[i].ttl <= now)
							{
								RemoveFromMessageLookup(_messagesWithTTL[i].sender, _messagesWithTTL[i].message.Receipt, false);
								_messagesWithTTL.RemoveAt(i--);
							}
						}

						if (_messagesWithTTL.Count == 0)
							break;
					}
					Thread.Sleep(10);
				}
			}
			catch (OperationCanceledException) { }
		}

		public void AddMessageSend(IMessage message, EndPoint receiver)
		{
			message.Receipt = GetNewMessageReceipt(receiver);
			AddMessage(message, receiver);
		}

		public void AddMessage(IMessage message, EndPoint sender)
		{
			if (message.Receipt == null)
				throw new MessageRepositoryMissingReceiptOnMessageException();
			if (Exists(sender, message.Receipt))
				throw new MessageWithReceiptSignatureAlreadyExistsException();
			lock (_messages)
			{
				if (!_messages.TryGetValue(sender, out var kv))
				{
					kv = new Dictionary<IMessageReceiptSignature, StoredMessage>();
					_messages.Add(sender, kv);
				}
				if (!_endpointRepeater.ContainsKey(sender))
				{
					_endpointRepeater.Add(sender, AbstractFactory.Get<INetworkTypeFactory>().GetNew<IForgeEndpointRepeater>());
				}
				message.IsBuffered = true;
				double resendDelay = Math.Max((double)_endpointRepeater[sender].Ping * ResendPingMultiplier, ResendMin);
				kv.Add(message.Receipt, new StoredMessage
				{
					TimeSent = DateTime.UtcNow,
					TimeResend = DateTime.UtcNow.AddMilliseconds(resendDelay),
					message = message
				});
			}
		}

		public void AddMessageSend(IMessage message, EndPoint receiver, int ttlMilliseconds)
		{
			message.Receipt = GetNewMessageReceipt(receiver);
			AddMessage(message, receiver, ttlMilliseconds);
		}

		public void AddMessage(IMessage message, EndPoint sender, int ttlMilliseconds)
		{
			if (ttlMilliseconds <= 0)
				throw new InvalidMessageRepositoryTTLProvided(ttlMilliseconds);

			AddMessage(message, sender);
			var span = new TimeSpan(0, 0, 0, 0, ttlMilliseconds);
			var now = DateTime.UtcNow;
			lock (_messagesWithTTL)
			{
				_messagesWithTTL.Add(new StoredTTLMessage
				{
					ttl = now + span,
					message = message,
					sender = sender
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
				if (_messages.TryGetValue(sender, out var kv))
                {
					foreach (var mkv in kv)
						copy.Add(mkv.Value.message);
				}
				_messages.Remove(sender);
			}

			try
			{
				foreach (var m in copy) m.Unbuffered();
				_endpointRepeater.Remove(sender);
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
				if (_messages.TryGetValue(sender, out var kv))
				{
					try
					{
						if (updatePing)
							_endpointRepeater[sender]?.AddPing(
								(int)((DateTime.UtcNow - kv[receipt].TimeSent).TotalMilliseconds));
						kv[receipt].message.Unbuffered();
						kv.Remove(receipt);
					}
					catch { } // Catch just in case message already removed
					kv.Remove(receipt);
				}
			}

			RemoveFromTTL(receipt);

		}

		private void RemoveFromMessageLookup(EndPoint sender, int hash)
		{
			lock (_messages)
			{
				if (_messages.TryGetValue(sender, out var kv))
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
				if (_messages.TryGetValue(sender, out var kv))
					exists = kv.ContainsKey(receipt);
			}
			return exists;
		}

		private void RemoveFromTTL(IMessageReceiptSignature receipt)
		{
			lock (_messagesWithTTL)
			{
				for (int i = 0; i < _messagesWithTTL.Count; i++)
				{
					if (_messagesWithTTL[i].message.Receipt != null) // find out why this happens
					{
						if (_messagesWithTTL[i].message.Receipt.Equals(receipt))
						{
							_messagesWithTTL.RemoveAt(i);
							break;
						}
					}
				}
			}
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
							copy.Add(new KeyValuePair<EndPoint, IMessage>(kv.Key, mkv.Value.message));
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
			if (!_endpointRepeater.ContainsKey(sender))
			{
				_endpointRepeater.Add(sender, AbstractFactory.Get<INetworkTypeFactory>().GetNew<IForgeEndpointRepeater>());
			}
			return _endpointRepeater[sender].ProcessReliableSignature(id);
		}

		private IMessageReceiptSignature GetNewMessageReceipt(EndPoint receiver)
		{
			if (!_endpointRepeater.ContainsKey(receiver))
			{
				_endpointRepeater.Add(receiver, AbstractFactory.Get<INetworkTypeFactory>().GetNew<IForgeEndpointRepeater>());
			}
			return _endpointRepeater[receiver].GetNewMessageReceipt();
		}
	}
}
