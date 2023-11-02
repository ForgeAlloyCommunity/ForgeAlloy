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
					Port = ForgeSerializer.Instance.Deserialize<ushort>(buffer),
					CurrentPlayers = ForgeSerializer.Instance.Deserialize<int>(buffer),
					MaxPlayers = ForgeSerializer.Instance.Deserialize<int>(buffer),
					PasswordProtected = ForgeSerializer.Instance.Deserialize<bool>(buffer),
					DedicatedServer = ForgeSerializer.Instance.Deserialize<bool>(buffer),

				};
			}
			return listing;
		}

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
				ForgeSerializer.Instance.Serialize(l.CurrentPlayers, buffer);
				ForgeSerializer.Instance.Serialize(l.MaxPlayers, buffer);
				ForgeSerializer.Instance.Serialize(l.PasswordProtected, buffer);
				ForgeSerializer.Instance.Serialize(l.DedicatedServer, buffer);

			}
		}


	}
}
