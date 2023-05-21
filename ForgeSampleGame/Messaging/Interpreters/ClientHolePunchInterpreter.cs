using System;
using System.Net;
using Forge.Networking;
using Forge.Networking.Messaging;
using Forge.ServerRegistry.Messaging.Interpreters;
using Forge.ServerRegistry.Messaging.Messages;

namespace ForgeSampleGame.Messaging.Interpreters
{
	public class ClientHolePunchInterpreter : IClientHolePunchInterpreter
	{
		public bool ValidOnClient => false;
		public bool ValidOnServer => true;

		public void Interpret(INetworkMediator netContainer, EndPoint sender, IMessage message)
		{
			var m = (ForgeClientHolePunchMessage)message;

			Console.WriteLine($"Server has punched a hole {m.ServerName}");

		}
	}
}
