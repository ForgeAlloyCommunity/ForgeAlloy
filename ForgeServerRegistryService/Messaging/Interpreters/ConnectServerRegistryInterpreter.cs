using System;
using System.Net;
using Forge.Networking;
using Forge.Networking.Messaging;
using Forge.Networking.Players;
using Forge.ServerRegistry.Messaging.Interpreters;
using Forge.ServerRegistry.Messaging.Messages;
using ForgeServerRegistryService.Networking.Players;

namespace ForgeServerRegistryService.Messaging.Interpreters
{
	public class ConnectServerRegistryInterpreter : IConnectServerRegistryInterpreter
	{
		public bool ValidOnClient => false;
		public bool ValidOnServer => true;

		public void Interpret(INetworkMediator netContainer, EndPoint sender, IMessage message)
		{
			RegisteredServer server = null;

			var m = (ForgeConnectServerRegistryMessage)message;

			if (sender is not IPEndPoint)
			{
				Console.WriteLine("Sender is not an IPEndPoint");
				return;
			}

			string senderAddress = ((IPEndPoint)sender).Address.MapToIPv4().ToString();
			ushort senderPort = (ushort)((IPEndPoint)sender).Port;

			var itr = netContainer.PlayerRepository.GetEnumerator();
			while (itr.MoveNext())
			{
				if (itr.Current != null)
				{
					if (((RegisteredServer)itr.Current).IP == m.ServerIp && ((RegisteredServer)itr.Current).Port == m.ServerPort)
					{
						server = (RegisteredServer)itr.Current;
						break;
					}
				}
			}

			if (server == null)
			{
				Console.WriteLine($"Server not found {m.ServerIp}:{m.ServerPort}");
			}

			// Send hole punch message to Game Server
			var holePunchMessage = new ForgeServerHolePunchMessage();
			holePunchMessage.PlayerIp = senderAddress;
			holePunchMessage.PlayerPort= senderPort;
			netContainer.MessageBus.SendReliableMessage(holePunchMessage, netContainer.SocketFacade.ManagedSocket, server.EndPoint);
			Console.WriteLine($"Hole punch requested for server {server.Name} to client {senderAddress}:{senderPort}");
		}
	}
}
