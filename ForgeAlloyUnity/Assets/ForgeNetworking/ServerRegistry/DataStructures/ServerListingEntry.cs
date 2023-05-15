namespace Forge.ServerRegistry.DataStructures
{
	public struct ServerListingEntry
	{
		public string Name { get; set; }
		public string Address { get; set; }
		public ushort Port { get; set; }

		public override string ToString()
		{
			return $"[{Address}:{Port}] {Name}";
		}
	}
}
