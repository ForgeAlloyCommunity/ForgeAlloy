using Forge.Factory;
using Forge.Networking.Messaging;
using Forge.Networking.Players;
using Forge.Serialization;
using Forge.ServerRegistry.Messaging.Interpreters;

namespace Forge.ServerRegistry.Messaging.Messages
{
	[ServerListingMessageContract(4, typeof(ForgeConnectServerRegistryMessage))]
	public class ForgeConnectServerRegistryMessage : ForgeMessage
	{
		public override IMessageInterpreter Interpreter =>
			AbstractFactory.Get<INetworkTypeFactory>().GetNew<IConnectServerRegistryInterpreter>();

		public string ServerIp { get; set; }
		public ushort ServerPort { get; set; }

		public override void Deserialize(BMSByte buffer)
		{
			ServerIp = ForgeSerializer.Instance.Deserialize<string>(buffer);
			ServerPort = ForgeSerializer.Instance.Deserialize<ushort>(buffer);
		}

		public override void Serialize(BMSByte buffer)
		{
			ForgeSerializer.Instance.Serialize(ServerIp, buffer);
			ForgeSerializer.Instance.Serialize(ServerPort, buffer);
		}
	}
}
