using System.Net;

namespace Forge.Networking.Sockets
{
	public interface ISocketNatFacade : ISocketFacade
	{
		void StartServer(ushort port, INetworkMediator netContainer);
	}
}
