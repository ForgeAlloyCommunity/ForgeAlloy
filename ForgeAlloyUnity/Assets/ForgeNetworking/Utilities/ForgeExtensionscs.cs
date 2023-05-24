using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Forge.Utilities
{
	public static class ForgeExtensionscs
	{
		public static long IPKey(this IPEndPoint ep)
		{
			var ipBytes = ep.Address.GetAddressBytes();
			var portBytes = BitConverter.GetBytes((ushort)ep.Port);
			byte[] buffer = new byte[8];
			System.Buffer.BlockCopy(ipBytes, 0, buffer, 0, ipBytes.Length);
			System.Buffer.BlockCopy(portBytes, 0, buffer, ipBytes.Length, portBytes.Length);
			return BitConverter.ToInt64(buffer, 0);
		}

		public static long IPKey(this EndPoint ep)
		{
			if (ep is IPEndPoint)
				return ((IPEndPoint)ep).IPKey();
			else
				return 0;
		}
	}
}
