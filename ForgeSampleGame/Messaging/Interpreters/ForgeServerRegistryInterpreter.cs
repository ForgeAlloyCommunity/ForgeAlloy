using System;
using System.Net;
using Forge.Factory;
using Forge.Networking;
using Forge.Networking.Messaging;
using Forge.Networking.Players;
using Forge.ServerRegistry.Messaging.Interpreters;
using Forge.ServerRegistry.Messaging.Messages;

namespace ForgeServerRegistryService.Messaging.Interpreters
{
	public class ForgeServerRegistryInterpreter : IServerRegistryInterpreter
	{
		public bool ValidOnClient => true;
		public bool ValidOnServer => false;

		public void Interpret(INetworkMediator netContainer, EndPoint sender, IMessage message)
		{
			ForgeServerRegistryMessage m = (ForgeServerRegistryMessage)message;

			Console.WriteLine($"ForgeServerRegistryMessage Count[{m.Entries.Length}]");
			foreach (var e in m.Entries)
			{
				Console.WriteLine(e.ToString());
			}
			Console.WriteLine(".");
		}
	}
}
