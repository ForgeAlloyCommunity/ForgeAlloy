using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Forge.Networking.Messaging;
using Forge.Networking.Messaging.Messages;
using Forge.Networking.Sockets;
using Forge.ServerRegistry.Messaging.Messages;

namespace Forge.Networking
{
	public class RegistryPinger
	{
		public int PingInterval { get; set; } = 5000;
		private INetworkMediator _networkMediator;
		private SynchronizationContext _sourceSyncCtx;
		EndPoint _registrationServerEndPoint;

		public void StartPinging(INetworkMediator networkMediator, IPEndPoint registrationEndPoint)
		{
			if (networkMediator.SocketFacade is ISocketClientFacade)
				throw new ArgumentException($"The RegistryPinger can only be used on a server");
			_sourceSyncCtx = SynchronizationContext.Current;
			_networkMediator = networkMediator;
			_registrationServerEndPoint = registrationEndPoint;
			Task.Run(PingAtInterval, _networkMediator.SocketFacade.CancellationSource.Token);
		}

		private void PingAtInterval()
		{
			try
			{
				while (true)
				{
					_networkMediator.SocketFacade.CancellationSource.Token.ThrowIfCancellationRequested();
					_sourceSyncCtx.Post(SendPingToServer, null);
					Thread.Sleep(PingInterval);
				}
			}
			catch (OperationCanceledException)
			{
				_networkMediator.EngineProxy.Logger.Log("Cancelling the background RegistryPinger task");
			}
		}

		private void SendPingToServer(object state)
		{
			ForgeRegisterAsServerMessage msg = ForgeMessageCodes.Instantiate<ForgeRegisterAsServerMessage>();
			msg.ServerName = _networkMediator.ServerName;
			msg.CurrentPlayers = _networkMediator.PlayerRepository.Count;
			msg.MaxPlayers = _networkMediator.MaxPlayers;

			_networkMediator.MessageBus.SendMessage(msg,
				_networkMediator.SocketFacade.ManagedSocket, _registrationServerEndPoint);
		}
	}
}
