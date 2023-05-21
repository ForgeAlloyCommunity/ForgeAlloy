using System;
using System.Net;
using Forge.Networking;
using Forge.Networking.Messaging;
using Forge.Networking.Sockets;
using Forge.ServerRegistry.Messaging.Interpreters;
using Forge.ServerRegistry.Messaging.Messages;

namespace ForgeSampleGameServer.Messaging.Interpreters
{
	public class ServerHolePunchInterpreter : IServerHolePunchInterpreter
	{
		public bool ValidOnClient => false;
		public bool ValidOnServer => true;

		public void Interpret(INetworkMediator netContainer, EndPoint sender, IMessage message)
		{
			var m = (ForgeServerHolePunchMessage)message;


			var clientEndPoint = ((IServerSocket)netContainer.SocketFacade.ManagedSocket).GetEndpoint(m.PlayerIp, m.PlayerPort);

			// Sending a message to the player on the port their NAT is sending on,
			// will  allow the server NAT to receive messages from the client on that port
			// i.e. Punch a hole
			var holePunchMessage = ForgeMessageCodes.Instantiate<ForgeClientHolePunchMessage>();
			holePunchMessage.ServerName = ForgeSampleGameServer.ServerName;
			netContainer.SendMessage(holePunchMessage, clientEndPoint);

			//BMSByte buffer = new BMSByte();
			//buffer.Append(new byte[] { 1 });
			//netContainer.SocketFacade.ManagedSocket.Send(clientEndPoint, buffer);

			Console.WriteLine($"Punching hole for {m.PlayerIp}:{m.PlayerPort}");

		}
	}
}
