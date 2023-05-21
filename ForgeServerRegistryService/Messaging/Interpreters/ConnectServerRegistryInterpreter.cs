using System;
using System.Net;
using Forge.Networking;
using Forge.Networking.Messaging;
using Forge.Networking.Players;
using Forge.ServerRegistry.Messaging.Interpreters;
using Forge.ServerRegistry.Messaging.Messages;
using ForgeServerRegistryService.Engine;
using ForgeServerRegistryService.Networking.Players;

namespace ForgeServerRegistryService.Messaging.Interpreters
{
	public class ConnectServerRegistryInterpreter : IConnectServerRegistryInterpreter
	{
		public bool ValidOnClient => false;
		public bool ValidOnServer => true;

		public void Interpret(INetworkMediator netContainer, EndPoint sender, IMessage message)
		{

			var m = (ForgeConnectServerRegistryMessage)message;

			if (sender is not IPEndPoint)
			{
				netContainer.Logger.Log("Sender is not an IPEndPoint");
				return;
			}

			// Hand back to server engine because the server knows about connected game hosts
			((NatEngine)netContainer.EngineProxy).ServerEngine.ConnectServerRegistry(m, sender);
		}
	}
}
