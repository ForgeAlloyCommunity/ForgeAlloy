using Forge.Networking.Players;

namespace Forge.ServerRegistry.DataStructures
{
	public struct ServerListingEntry
	{
		public IPlayerSignature Id { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public ushort Port { get; set; }
		public int CurrentPlayers { get; set; }
		public int MaxPlayers { get; set; }
		public bool PasswordProtected { get; set; }
		public bool DedicatedServer { get; set; }

		public override string ToString()
		{
			return $"{Name}({Id}) [{Address}:{Port}]";
		}
	}
}
