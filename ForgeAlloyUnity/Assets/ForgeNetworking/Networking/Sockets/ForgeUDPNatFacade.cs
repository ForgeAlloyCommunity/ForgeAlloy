using Forge.Factory;
using Forge.Networking.Messaging.Messages;
using Forge.Networking.Players;
using Forge.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Forge.Networking.Sockets
{
	public class ForgeUDPNatFacade : ForgeUDPSocketFacadeBase, ISocketNatFacade, ISocketServerFacade
	{
		private const int CHALLENGED_PLAYER_TTL = 5000;
		private const int MAX_PARALLEL_CONNECTION_REQUEST = 64;

		private readonly IServerSocket _socket;
		public override ISocket ManagedSocket => _socket;
		private readonly List<EndPoint> _bannedEndpoints = new List<EndPoint>();

		public IPlayerSignature NetPlayerId { get; set; }


		public ForgeUDPNatFacade()
		{
			_socket = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IServerSocket>();
		}

		public void StartServer(ushort port, INetworkMediator netMediator)
		{
			networkMediator = netMediator;
			_socket.Listen(port, MAX_PARALLEL_CONNECTION_REQUEST);
			CancellationSource = new CancellationTokenSource();
			Task.Run(ReadNetwork, CancellationSource.Token);
		}

		public override void ShutDown()
		{
			CancellationSource.Cancel();
			base.ShutDown();
		}

		public void ChallengeSuccess(INetworkMediator netContainer, EndPoint endpoint)
		{
		}

		protected override void ProcessMessageRead(BMSByte buffer, EndPoint sender)
		{
			if (_bannedEndpoints.Contains(sender))
				return;

			ProcessPlayerMessageRead(sender, buffer);
		}
		protected void ProcessPlayerMessageRead(EndPoint sender, BMSByte buffer)
		{
			networkMediator.MessageBus.ReceiveMessageBuffer(ManagedSocket, sender, buffer);
		}

		#region ISocketServerFacade

		// Required for ISocketServerFacade interface. But never used
		// THis is so that IsServer returns true on the network mediator

		public void StartServer(ushort port, int maxPlayers, INetworkMediator netContainer)
		{
			throw new NotImplementedException();
		}

		public void StartServerWithRegistration(ushort port, int maxPlayers, INetworkMediator netMediator, string registrationServerAddress, ushort registrationServerPort, string serverName)
		{
			throw new NotImplementedException();
		}

		#endregion ISocketServerFacade

	}
}
