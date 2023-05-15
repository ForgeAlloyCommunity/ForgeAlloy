using System.Net;

namespace Forge.Networking.Sockets
{
	public interface ISocketServerFacade : ISocketFacade
	{
		void StartServer(ushort port, int maxPlayers, INetworkMediator netContainer);
		void StartServerWithRegistration(ushort port, int maxPlayers, INetworkMediator netMediator, string registrationServerAddress, ushort registrationServerPort, string serverName);
		void ChallengeSuccess(INetworkMediator netContainer, EndPoint endpoint);
	}
}
