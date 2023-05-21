using System.Net;
using Forge.Engine;
using Forge.Networking.Messaging;
using Forge.Networking.Players;
using Forge.Networking.Sockets;

namespace Forge.Networking
{
	public interface INetworkMediator
	{
		int PlayerTimeout { get; }
		IPlayerRepository PlayerRepository { get; }
		IEngineProxy EngineProxy { get; }
		IMessageBus MessageBus { get; }
		ISocketFacade SocketFacade { get; }
		bool IsClient { get; }
		bool IsServer { get; }
		void ChangeEngineProxy(IEngineProxy engineProxy);
		void StartServer(ushort port, int maxPlayers);
		void StartServerWithRegistration(ushort port, int maxPlayers, string registrationServerAddress, ushort registrationServerPort, string serverName);
		void StartNatServer(ushort port);
		void StartClient(string hostAddress, ushort port);
		void StartClientWithNat(string hostAddress, ushort port, string registryServerAddress, ushort registryServerPort);
		void SendMessage(IMessage message);
		void SendMessage(IMessage message, INetPlayer player);
		void SendMessage(IMessage message, EndPoint endpoint);
		void SendReliableMessage(IMessage message);
		void SendReliableMessage(IMessage message, INetPlayer player);
		void SendReliableMessage(IMessage message, EndPoint endpoint, int ttlMilliseconds = 0);
		int MaxPlayers { get; }
		string ServerName { get; }
		IForgeLogger Logger { get; }
	}
}
