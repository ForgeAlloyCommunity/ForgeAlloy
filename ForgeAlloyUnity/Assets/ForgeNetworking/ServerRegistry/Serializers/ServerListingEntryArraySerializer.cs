using System;
using System.Text;
using Forge.Factory;
using Forge.Networking.Players;
using Forge.Serialization;
using Forge.ServerRegistry.DataStructures;

namespace Forge.ServerRegistry.Serializers
{
	public class ServerListingEntryArraySerializer : ITypeSerializer
	{
		//public object Deserialize(BMSByte buffer)
		//{
		//	int length = buffer.GetBasicType<int>();
		//	ServerListingEntry[] listing = new ServerListingEntry[length];
		//	for (int i = 0; i < length; i++)
		//	{
		//		listing[i] = new ServerListingEntry
		//		{
		//			Id = ForgeSerializer.Instance.Deserialize<IPlayerSignature>(buffer),
		//			Name = buffer.GetBasicType<string>(),
		//			Address = buffer.GetBasicType<string>(),
		//			Port = buffer.GetBasicType<ushort>()
		//		};
		//	}
		//	return listing;
		//}
		public object Deserialize(BMSByte buffer)
		{
			int length = ForgeSerializer.Instance.Deserialize<int>(buffer);
			ServerListingEntry[] listing = new ServerListingEntry[length];
			for (int i = 0; i < length; i++)
			{
				listing[i] = new ServerListingEntry
				{
					Id = ForgeSerializer.Instance.Deserialize<IPlayerSignature>(buffer),
					Name = ForgeSerializer.Instance.Deserialize<string>(buffer),
					Address = ForgeSerializer.Instance.Deserialize<string>(buffer),
					Port = ForgeSerializer.Instance.Deserialize<ushort>(buffer)
				};
			}
			return listing;
		}

		//public void Serialize(object val, BMSByte buffer)
		//{
		//	var listing = (ServerListingEntry[])val;
		//	buffer.Append(BitConverter.GetBytes(listing.Length));
		//	foreach (var l in listing)
		//	{
		//		l.Id.Serialize(buffer);
		//		byte[] strBuf = Encoding.UTF8.GetBytes(l.Name ?? string.Empty);
		//		buffer.Append(BitConverter.GetBytes(strBuf.Length));
		//		buffer.Append(strBuf);
		//		strBuf = Encoding.UTF8.GetBytes(l.Address ?? string.Empty);
		//		buffer.Append(BitConverter.GetBytes(strBuf.Length));
		//		buffer.Append(strBuf);
		//		buffer.Append(BitConverter.GetBytes(l.Port));
		//	}
		//}

		public void Serialize(object val, BMSByte buffer)
		{
			var listing = (ServerListingEntry[])val;
			ForgeSerializer.Instance.Serialize(listing.Length, buffer);
			foreach (var l in listing)
			{
				ForgeSerializer.Instance.Serialize(l.Id, buffer);
				ForgeSerializer.Instance.Serialize(l.Name, buffer);
				ForgeSerializer.Instance.Serialize(l.Address, buffer);
				ForgeSerializer.Instance.Serialize(l.Port, buffer);
			}
		}


	}
}
