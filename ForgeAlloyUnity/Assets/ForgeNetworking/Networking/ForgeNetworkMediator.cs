using System;
using System.Net;
using Forge.Engine;
using Forge.Factory;
using Forge.Networking.Messaging;
using Forge.Networking.Players;
using Forge.Networking.Sockets;
using Forge.ServerRegistry.Messaging.Messages;

namespace Forge.Networking
{
	public class ForgeNetworkMediator : INetworkMediator
	{
		public int PlayerTimeout => _timeoutBridge.TimeoutMilliseconds;
		public IPlayerRepository PlayerRepository { get; private set; }
		public IEngineProxy EngineProxy { get; private set; }
		public IMessageBus MessageBus { get; private set; }
		public ISocketFacade SocketFacade { get; private set; }
		public bool IsClient => SocketFacade is ISocketClientFacade;
		public bool IsServer => SocketFacade is ISocketServerFacade;

		private readonly IPlayerTimeoutBridge _timeoutBridge;
		public int MaxPlayers { get; private set; }
		public string ServerName { get; private set; }
		public IForgeLogger Logger => EngineProxy.Logger;

		public ForgeNetworkMediator()
		{
			PlayerRepository = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IPlayerRepository>();
			MessageBus = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageBus>();
			_timeoutBridge = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IPlayerTimeoutBridge>();
		}

		public void ChangeEngineProxy(IEngineProxy engineProxy)
		{
			EngineProxy = engineProxy;
		}

		public void StartServer(ushort port, int maxPlayers)
		{
			StartServerWithRegistration(port, maxPlayers, "", 0, "");
		}

		public void StartServerWithRegistration(ushort port, int maxPlayers, string registrationServerAddress, ushort registrationServerPort, string serverName)
		{
			MaxPlayers = maxPlayers;
			ServerName = serverName;

			var server = AbstractFactory.Get<INetworkTypeFactory>().GetNew<ISocketServerFacade>();
			SocketFacade = server;
			if (string.IsNullOrEmpty(registrationServerAddress))
				server.StartServer(port, maxPlayers, this);
			else
				server.StartServerWithRegistration(port, maxPlayers, this, registrationServerAddress, registrationServerPort, serverName);
			_timeoutBridge.StartWatching(this);
			EngineProxy.NetworkingEstablished();
			MessageBus.SetMediator(this);
		}

		public void StartNatServer(ushort port)
		{
			ServerName = "Nat";
			var server = AbstractFactory.Get<INetworkTypeFactory>().GetNew<ISocketNatFacade>();
			SocketFacade = server;
			server.StartServer(port, this);
			EngineProxy.NetworkingEstablished();
			MessageBus.SetMediator(this);
		}

		public void StartClient(string hostAddress, ushort port)
		{
			StartClientWithNat(hostAddress, port, string.Empty, 0);
		}
		public void StartClientWithNat(string hostAddress, ushort port, string registrationServerAddress, ushort natPort)
		{
			var client = AbstractFactory.Get<INetworkTypeFactory>().GetNew<ISocketClientFacade>();
			SocketFacade = client;

			if (natPort != 0)
			{
				HolePunch(hostAddress, port, registrationServerAddress, natPort);
			}

			client.StartClient(hostAddress, port, this);
			MessageBus.SetMediator(this);
		}

		/// <summary>
		/// Send a message to the registration server requesting a NAT Hole punch
		/// that will be sent to the server to punch a hole through its NAT
		/// </summary>
		/// <param name="hostAddress"></param>
		/// <param name="port"></param>
		/// <param name="registrationServerAddress"></param>
		/// <param name="natPort"></param>
		private void HolePunch(string hostAddress, ushort port, string registrationServerAddress, ushort natPort)
		{
			var registrationEndPoint = SocketFacade.ManagedSocket.GetEndpoint(registrationServerAddress, natPort);
			ForgeConnectServerRegistryMessage connectionRequest = ForgeMessageCodes.Instantiate<ForgeConnectServerRegistryMessage>();
			connectionRequest.ServerIp = hostAddress;
			connectionRequest.ServerPort = port;

			SendReliableMessage(connectionRequest, registrationEndPoint);
		}

		public void SendMessage(IMessage message)
		{
			if (SocketFacade is ISocketServerFacade)
			{
				var itr = PlayerRepository.GetEnumerator();
				message.StartSending();
				while (itr.MoveNext())
				{
					if (itr.Current != null)
						MessageBus.SendMessage(message, SocketFacade.ManagedSocket, itr.Current.EndPoint);
				}
				message.FinishSending();
			}
			else
				MessageBus.SendMessage(message, SocketFacade.ManagedSocket, SocketFacade.ManagedSocket.EndPoint);
		}

		public void SendMessage(IMessage message, INetPlayer player)
		{
			MessageBus.SendMessage(message, SocketFacade.ManagedSocket, player.EndPoint);
		}

		public void SendMessage(IMessage message, EndPoint endpoint)
		{
			MessageBus.SendMessage(message, SocketFacade.ManagedSocket, endpoint);
		}

		public void SendReliableMessage(IMessage message)
		{
			if (SocketFacade is ISocketServerFacade)
			{
				var itr = PlayerRepository.GetEnumerator();
				while (itr.MoveNext())
				{
					if (itr.Current != null)
						MessageBus.SendReliableMessage(message, SocketFacade.ManagedSocket, itr.Current.EndPoint);
				}
			}
			else
				MessageBus.SendReliableMessage(message, SocketFacade.ManagedSocket, SocketFacade.ManagedSocket.EndPoint);
		}

		public void SendReliableMessage(IMessage message, INetPlayer player)
		{
			MessageBus.SendReliableMessage(message, SocketFacade.ManagedSocket, player.EndPoint);
		}

		public void SendReliableMessage(IMessage message, EndPoint endpoint, int ttlMilliseconds = 0)
		{
			MessageBus.SendReliableMessage(message, SocketFacade.ManagedSocket, endpoint, ttlMilliseconds);
		}
	}
}
