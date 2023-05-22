using Forge;
using Forge.Factory;
using Forge.Networking.Players;
using Forge.Networking.Sockets;
using Forge.ServerRegistry.DataStructures;
using Forge.ServerRegistry.Serializers;
using NUnit.Framework;

namespace ForgeTests.Serialization.Serializers
{
	[TestFixture]
	public class ServerListingEntryArraySerializerTests : TypeSerializerTestsBase
	{
		[Test]
		public void SerializingServerListingEntryArray_ShouldBeEqual()
		{
			ForgeRegistration.Initialize();
			ServerListingEntry[] input = new ServerListingEntry[]
			{
				new ServerListingEntry
				{
					Id = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IPlayerSignature>(),
					Name = "Server1",
					Address = "10.2.5.96",
					Port = 1265
				},
				new ServerListingEntry
				{
					Id = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IPlayerSignature>(),
					Name = "Server2",
					 Address = CommonSocketBase.LOCAL_IPV4,
					 Port = 15937
				},
				new ServerListingEntry
				{
					Id = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IPlayerSignature>(),
					Name = "Server3",
					Address = CommonSocketBase.LOCAL_ANY_IPV4,
					Port = 35719
				},
				new ServerListingEntry
				{
					Id = AbstractFactory.Get < INetworkTypeFactory >().GetNew < IPlayerSignature >(),
					Name = "Server4",
					Address = "76.36.45.25",
					Port = 555
				},
				new ServerListingEntry
				{
					Id = AbstractFactory.Get < INetworkTypeFactory >().GetNew < IPlayerSignature >(),
					Name = "Server5",
					Address = "255.0.2.255",
					Port = 983
				}
			};
			ServerListingEntry[] res = SerializeDeserialize(new ServerListingEntryArraySerializer(), input);
			Assert.AreEqual(input.Length, res.Length);
			for (int i = 0; i < input.Length; i++)
			{
				Assert.AreEqual(input[i].Id, res[i].Id);
				Assert.AreEqual(input[i].Name, res[i].Name);
				Assert.AreEqual(input[i].Address, res[i].Address);
				Assert.AreEqual(input[i].Port, res[i].Port);
			}
			ForgeRegistration.Teardown();
		}
	}
}
