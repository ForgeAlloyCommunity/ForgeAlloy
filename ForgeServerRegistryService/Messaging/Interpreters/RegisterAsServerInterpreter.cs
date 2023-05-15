using System;
using System.Net;
using Forge.Factory;
using Forge.Networking;
using Forge.Networking.Messaging;
using Forge.Networking.Players;
using Forge.ServerRegistry.Messaging.Interpreters;
using Forge.ServerRegistry.Messaging.Messages;
using ForgeServerRegistryService.Networking.Players;

namespace ForgeServerRegistryService.Messaging.Interpreters
{
	public class RegisterAsServerInterpreter : IRegisterAsServerInterpreter
	{
		public bool ValidOnClient => false;
		public bool ValidOnServer => true;

		public void Interpret(INetworkMediator netContainer, EndPoint sender, IMessage message)
		{
			ForgeRegisterAsServerMessage m = (ForgeRegisterAsServerMessage)message;

			if (!netContainer.PlayerRepository.Exists(sender))
			{
				var newServer = AbstractFactory.Get<INetworkTypeFactory>().GetNew<INetPlayer>();
				newServer.EndPoint = sender;
				var registeredServer = (RegisteredServer)newServer;
				registeredServer.IsRegisteredServer = true;
				registeredServer.Name = m.ServerName;
				registeredServer.LastCommunication = DateTime.Now;
				registeredServer.MaxPlayers = m.MaxPlayers;
				registeredServer.CurrentPlayers = m.CurrentPlayers;
				netContainer.PlayerRepository.AddPlayer(newServer);
			}
			else
			{
				var registeredServer = (RegisteredServer)netContainer.PlayerRepository.GetPlayer(sender);
				registeredServer.LastCommunication = DateTime.Now;
				registeredServer.MaxPlayers = m.MaxPlayers;
				registeredServer.CurrentPlayers = m.CurrentPlayers;
			}


			Console.WriteLine($"RegisterAsServerMessage {m.ServerName} {m.CurrentPlayers}/{m.MaxPlayers}");
		}
	}
}
