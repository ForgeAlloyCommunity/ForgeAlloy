﻿using System;
using System.Net;
using Forge.Networking.Players;

namespace ForgeServerRegistryService.Networking.Players
{
	public class RegisteredServer : INetPlayer
	{
		public EndPoint EndPoint { get; set; }
		public IPlayerSignature Id { get; set; }
		public string Name { get; set; }
		public bool IsInEngine { get; set; }
		public bool IsRegisteredServer { get; set; }
		public DateTime LastCommunication { get; set; }
		public int MaxPlayers { get; set; }
		public int CurrentPlayers { get; set; }
	}
}
