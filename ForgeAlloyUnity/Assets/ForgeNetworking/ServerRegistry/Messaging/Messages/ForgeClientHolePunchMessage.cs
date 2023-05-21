using Forge.Factory;
using Forge.Networking.Messaging;
using Forge.Serialization;
using Forge.ServerRegistry.DataStructures;
using Forge.ServerRegistry.Messaging.Interpreters;

namespace Forge.ServerRegistry.Messaging.Messages
{
	[ServerListingMessageContract(6, typeof(ForgeClientHolePunchMessage))]
	public class ForgeClientHolePunchMessage : ForgeMessage
	{
		public string ServerName { get; set; }

		public override IMessageInterpreter Interpreter =>
			AbstractFactory.Get<INetworkTypeFactory>().GetNew<IClientHolePunchInterpreter>();

		public override void Deserialize(BMSByte buffer)
		{
			ServerName = ForgeSerializer.Instance.Deserialize<string>(buffer);
		}

		public override void Serialize(BMSByte buffer)
		{
			ForgeSerializer.Instance.Serialize(ServerName, buffer);
		}
	}
}
