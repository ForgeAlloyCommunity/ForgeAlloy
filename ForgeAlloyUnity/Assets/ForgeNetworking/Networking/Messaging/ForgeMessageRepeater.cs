using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Forge.Factory;

namespace Forge.Networking.Messaging
{
	public class ForgeMessageRepeater : IMessageRepeater
	{
		public int RepeatMillisecondsInterval { get; set; } = 16;
		private INetworkMediator _networkMediator;

		private readonly IMessageRepository _messageRepository;
		private CancellationTokenSource _socketTokenSourceRef;

		public ForgeMessageRepeater()
		{
			_messageRepository = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageRepository>();
		}

		public void Start(INetworkMediator networkMediator)
		{
			_networkMediator = networkMediator;
			_messageRepository.Id = $"[{_networkMediator.EngineProxy.Id}]Repeater";
			_socketTokenSourceRef = _networkMediator.SocketFacade.CancellationSource;
			Task.Run(RepeatInBackground, _socketTokenSourceRef.Token);
		}

		public void AddMessageToRepeat(IMessage message, EndPoint receiver, int ttlMilliseconds = 0)
		{
			//if (!_messageRepository.Exists(receiver, message.Receipt))
				_messageRepository.AddMessage(message, receiver, ttlMilliseconds);
		}

		public void RemoveRepeatingMessage(EndPoint sender, IMessageReceiptSignature messageReceipt, ushort recentPackets)
		{
			_messageRepository.RemoveMessage(sender, messageReceipt, recentPackets);
		}

		public void RemoveAllFor(EndPoint receiver)
		{
			_messageRepository.RemoveAllFor(receiver);
		}

		private void RepeatInBackground()
		{
			// This starts when a reliable message is first created, so because of that
			// we should wait by the interval when we first enter this method
			Thread.Sleep(RepeatMillisecondsInterval);
			try
			{
				while (true)
				{
					_socketTokenSourceRef.Token.ThrowIfCancellationRequested();
					_messageRepository.Iterate(ResendMessage);
					Thread.Sleep(RepeatMillisecondsInterval);
				}
			}
			catch (OperationCanceledException)
			{
				_networkMediator.EngineProxy.Logger.Log("Cancelling the message repeater background task");
			}
		}

		private void ResendMessage(EndPoint endpoint, IMessage message)
		{
			_networkMediator.MessageBus.SendMessage(message,
				_networkMediator.SocketFacade.ManagedSocket, endpoint);
		}

		public IMessageReceiptSignature GetNewMessageReceipt(EndPoint receiver)
		{
			return _messageRepository.GetNewMessageReceipt(receiver);
		}
	}
}
