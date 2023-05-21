using Forge.Factory;
using Forge.Networking.Messaging;
using Forge.Serialization;
using Forge.ServerRegistry.DataStructures;
using Forge.ServerRegistry.Messaging.Interpreters;

namespace Forge.ServerRegistry.Messaging.Messages
{
	[ServerListingMessageContract(5, typeof(ForgeServerHolePunchMessage))]
	public class ForgeServerHolePunchMessage : ForgeMessage
	{
		public string PlayerIp { get; set; }
		public ushort PlayerPort { get; set; }

		public override IMessageInterpreter Interpreter =>
			AbstractFactory.Get<INetworkTypeFactory>().GetNew<IServerHolePunchInterpreter>();

		public override void Deserialize(BMSByte buffer)
		{
			PlayerIp = ForgeSerializer.Instance.Deserialize<string>(buffer);
			PlayerPort = ForgeSerializer.Instance.Deserialize<ushort>(buffer);
		}

		public override void Serialize(BMSByte buffer)
		{
			ForgeSerializer.Instance.Serialize(PlayerIp, buffer);
			ForgeSerializer.Instance.Serialize(PlayerPort, buffer);
		}
	}
}
