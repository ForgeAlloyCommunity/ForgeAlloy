using Forge.Networking.Players;

namespace Forge.ServerRegistry.DataStructures
{
	public struct ServerListingEntry
	{
		public IPlayerSignature Id { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public ushort Port { get; set; }

		public override string ToString()
		{
			return $"{Name}({Id}) [{Address}:{Port}]";
		}
	}
}
