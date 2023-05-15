using System.Net;

namespace Forge.Networking.Sockets
{
	public interface IServerSocket : ISocket
	{
		void Listen(ushort port, int maxParallelConnections);
		IPEndPoint GetEndpoint(string address, ushort port);
	}
}
