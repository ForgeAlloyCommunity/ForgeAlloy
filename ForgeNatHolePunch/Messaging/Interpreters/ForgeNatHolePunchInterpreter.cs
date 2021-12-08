using System.Net;
using Forge.Networking;
using Forge.Networking.Messaging;

namespace ForgeNatHolePunch.Messaging.Interpreters
{
	public class ForgeNatHolePunchInterpreter : IMessageInterpreter
	{
		public static ForgeNatHolePunchInterpreter Instance { get; private set; } = new ForgeNatHolePunchInterpreter();

		public bool ValidOnClient => true;
		public bool ValidOnServer => true;

		public void Interpret(INetworkMediator netContainer, EndPoint sender, IMessage message)
		{
			//Not yet implemented, not sure what this should do yet.
		}
	}
}
