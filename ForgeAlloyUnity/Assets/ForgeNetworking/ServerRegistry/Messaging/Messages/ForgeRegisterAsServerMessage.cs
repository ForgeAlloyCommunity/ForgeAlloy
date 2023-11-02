using Forge.Factory;
using Forge.Networking.Messaging;
using Forge.Serialization;
using Forge.ServerRegistry.Messaging.Interpreters;

namespace Forge.ServerRegistry.Messaging.Messages
{
	[ServerListingMessageContract(2, typeof(ForgeRegisterAsServerMessage))]
	public class ForgeRegisterAsServerMessage : ForgeMessage
	{
		public string ServerName { get; set; }
		public ushort Port { get; set; }
		public int MaxPlayers { get; set; }
		public int CurrentPlayers { get; set; }
		public bool PasswordProtected { get; set; }
		public bool DedicatedServer { get; set; }

		public override IMessageInterpreter Interpreter =>
			AbstractFactory.Get<INetworkTypeFactory>().GetNew<IRegisterAsServerInterpreter>();

		public override void Deserialize(BMSByte buffer)
		{
			ServerName = ForgeSerializer.Instance.Deserialize<string>(buffer);
			Port = ForgeSerializer.Instance.Deserialize<ushort>(buffer);
			MaxPlayers = ForgeSerializer.Instance.Deserialize<int>(buffer);
			CurrentPlayers = ForgeSerializer.Instance.Deserialize<int>(buffer);
			PasswordProtected = ForgeSerializer.Instance.Deserialize<bool>(buffer);
			DedicatedServer = ForgeSerializer.Instance.Deserialize<bool>(buffer);
		}

		public override void Serialize(BMSByte buffer)
		{
			ForgeSerializer.Instance.Serialize(ServerName, buffer);
			ForgeSerializer.Instance.Serialize(Port, buffer);
			ForgeSerializer.Instance.Serialize(MaxPlayers, buffer);
            ForgeSerializer.Instance.Serialize(CurrentPlayers, buffer);
            ForgeSerializer.Instance.Serialize(PasswordProtected, buffer);
			ForgeSerializer.Instance.Serialize(DedicatedServer, buffer);
		}
	}
}
