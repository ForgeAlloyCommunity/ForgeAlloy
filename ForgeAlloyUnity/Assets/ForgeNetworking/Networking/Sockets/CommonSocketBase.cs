using System;
using System.Net;
using System.Net.Sockets;

namespace Forge.Networking.Sockets
{
	public class CommonSocketBase
	{
		public const string LOCALHOST = "localhost";
		public const string LOCAL_IPV4 = "127.0.0.1";
		public const string LOCAL_ANY_IPV4 = "0.0.0.0";

		protected static IPEndPoint GetEndpoint(string address, ushort port)
		{
			if (address.ToLower() == LOCALHOST)
				address = LOCAL_IPV4;

			if (address == LOCAL_IPV4)
				return new IPEndPoint(IPAddress.Parse(address), port);
			else
				return LocateAssociatedIP(address, port);
		}

		private static IPEndPoint LocateAssociatedIP(string address, ushort port)
		{
			try
			{
				string host = string.IsNullOrEmpty(address) ? Dns.GetHostName() : address;
				IPHostEntry ipHostInfo = Dns.GetHostEntry(host);

				// TODO:  Support IPv6
				IPAddress ipAddress = null;
				for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
				{
					ipAddress = ipHostInfo.AddressList[i];
					if (ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
					{
						break;
					}
				}

				var localEndPoint = new IPEndPoint(ipAddress, port);
				return localEndPoint;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			if (IPAddress.TryParse(address, out var ip))
			{
				IPEndPoint ep = new IPEndPoint(ip, port);
				return ep;
			}
			throw new Exception("Cannot parse IP Address");

		}
	}
}
