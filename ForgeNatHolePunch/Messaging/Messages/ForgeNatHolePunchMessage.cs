using Forge.Networking.Messaging;
using ForgeNatHolePunch.Messaging.Interpreters;
using Forge.Serialization;

namespace ForgeNatHolePunch.Messaging.Messages
{
	//This could be a 1-message-fits-all
	public class ForgeNatHolePunchMessage : ForgeMessage
	{
		public override IMessageInterpreter Interpreter => ForgeNatHolePunchInterpreter.Instance;

		public override void Deserialize(BMSByte buffer)
		{

		}

		public override void Serialize(BMSByte buffer)
		{

		}
	}
}
