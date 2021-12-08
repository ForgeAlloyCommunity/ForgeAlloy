using System;
using System.Net;
using Forge.Networking.Players;

//This is a port of NatHolePunch from Forge Networking Remastered, almost one-to-one.
//I am not sure if this will be useful in Alloy's implementation, but I think it will.
namespace ForgeNatHolePunch
{
	public struct Host
	{
		public ForgePlayer player;
		public IPEndPoint endpoint;
		public IPAddress address;
		public string host;
		public ushort port;

		public Host(ForgePlayer player, string host, ushort port)
		{
			this.player = player;
			this.host = host;
			this.port = port;

			if (IPAddress.TryParse(host, out address))
			{
				endpoint = new IPEndPoint(address, port);
			}
			else
			{
				throw new Exception("Invalid host address");
			}
		}
	}
}
