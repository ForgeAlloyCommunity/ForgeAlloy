using System;
using System.Net;
using Forge.Engine;
using Forge.Networking;
using Forge.ServerRegistry.Messaging.Messages;
using ForgeServerRegistryService.Networking.Players;

namespace ForgeServerRegistryService.Engine
{
	public class ServerRegistryEngine : IEngineProxy
	{
		private IForgeLogger logger = new ForgeConsoleLogger();
		public IForgeLogger Logger => logger;
		public string Id => "ServerRegistry";

		public INetworkMediator NetContainer { get; set; }

		public bool CanConnectToChallenge()
		{
			return true;
		}

		public void NetworkingEstablished()
		{
			// Not expecting to have any entities on the server registry engine
			return;
		}

		public void ConnectServerRegistry(ForgeConnectServerRegistryMessage m, EndPoint sender)
		{
			RegisteredServer server = null;

			string senderAddress = ((IPEndPoint)sender).Address.MapToIPv4().ToString();
			ushort senderPort = (ushort)((IPEndPoint)sender).Port;


			var itr = NetContainer.PlayerRepository.GetEnumerator();
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

			if (server != null)
			{
				// Send hole punch message to Game Server
				var holePunchMessage = new ForgeServerHolePunchMessage();
				holePunchMessage.PlayerIp = senderAddress;
				holePunchMessage.PlayerPort = senderPort;
				NetContainer.MessageBus.SendReliableMessage(holePunchMessage, NetContainer.SocketFacade.ManagedSocket, server.EndPoint);
				logger.Log($"Hole punch requested for server {server.Name} to client {senderAddress}:{senderPort}");
			}
			else
			{
				logger.Log($"Server not found {m.ServerIp}:{m.ServerPort}. Sender: {sender}");
			}
		}
	}
}
