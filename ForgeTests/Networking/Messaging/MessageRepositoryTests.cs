using System.Net;
using System.Threading;
using FakeItEasy;
using Forge.Factory;
using Forge.Networking.Messaging;
using NUnit.Framework;

namespace ForgeTests.Networking.Messaging
{
	[TestFixture]
	public class MessageRepositoryTests : ForgeNetworkingTest
	{
		[Test]
		public void AddMessage_ShouldExist()
		{
			var message = A.Fake<IMessage>();
			var endpoint = A.Fake<EndPoint>();
			message.Receipt = A.Fake<IMessageReceiptSignature>();
			var repo = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageRepository>();
			repo.AddMessage(message, endpoint);
			Assert.IsTrue(repo.Exists(endpoint, message.Receipt));
		}

		[Test]
		public void AddMessageWithoutReceipt_ShouldThrow()
		{
			var message = A.Fake<IMessage>();
			message.Receipt = null;
			var repo = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageRepository>();
			Assert.Throws<MessageRepositoryMissingReceiptOnMessageException>(() => repo.AddMessage(message, A.Fake<EndPoint>()));
		}

		[Test]
		public void AddMessageWithSameGuid_ShouldThrow()
		{
			var message = A.Fake<IMessage>();
			var endpoint = A.Fake<EndPoint>();
			message.Receipt = A.Fake<IMessageReceiptSignature>();
			var repo = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageRepository>();
			repo.AddMessage(message, endpoint);
			Assert.Throws<MessageWithReceiptSignatureAlreadyExistsException>(() => repo.AddMessage(message, endpoint));
		}

		[Test]
		public void CheckForMessage_ShouldNotExist()
		{
			var fake = A.Fake<IMessageReceiptSignature>();
			var endpoint = A.Fake<EndPoint>();
			var repo = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageRepository>();
			Assert.IsFalse(repo.Exists(endpoint, fake));
		}

		[Test]
		public void RemoveMessage_ShouldBeRemoved()
		{
			var message = A.Fake<IMessage>();
			var endpoint = A.Fake<EndPoint>();
			message.Receipt = A.Fake<IMessageReceiptSignature>();
			var repo = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageRepository>();
			repo.AddMessage(message, endpoint);
			Assert.IsTrue(repo.Exists(endpoint, message.Receipt));
			repo.RemoveMessage(endpoint, message.Receipt);
			Assert.IsFalse(repo.Exists(endpoint, message.Receipt));
			repo.AddMessage(message, endpoint);
			Assert.IsTrue(repo.Exists(endpoint, message.Receipt));
			repo.RemoveMessage(endpoint, message);
			Assert.IsFalse(repo.Exists(endpoint, message.Receipt));
		}

		[Test, MaxTime(1000)]
		public void MessageAfterTTL_ShouldBeRemoved()
		{
			var message = A.Fake<IMessage>();
			var endpoint = A.Fake<EndPoint>();
			message.Receipt = A.Fake<IMessageReceiptSignature>();
			var repo = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageRepository>();
			// This test can be sensitive to the TTL.
			// If it is too short when all tests are run, this test will fail.
			// 10 fails sometimes, but 50 seems ok
			repo.AddMessage(message, endpoint, 50);
			Assert.IsTrue(repo.Exists(endpoint, message.Receipt));
			Thread.Sleep(1);
			Assert.IsTrue(repo.Exists(endpoint, message.Receipt));
			bool removed;
			do
			{
				removed = repo.Exists(endpoint, message.Receipt);
			} while (!removed);
			Assert.IsTrue(removed);
		}

		// Test no longer valid. The addmessage will auto-correct -ve or sero TTL
		//[Test]
		//public void AddMessageWithNegativeTTL_ShouldThrow()
		//{
		//	var repo = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageRepository>();
		//	Assert.Throws<InvalidMessageRepositoryTTLProvided>(() => repo.AddMessage(A.Fake<IMessage>(), A.Fake<EndPoint>(), -1));
		//}

		// Test no longer valid. The addmessage will auto-correct -ve or sero TTL
		// Test no longer valid
		//[Test]
		//public void AddMessageWithZeroTTL_ShouldThrow()
		//{
		//	var repo = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageRepository>();
		//	Assert.Throws<InvalidMessageRepositoryTTLProvided>(() => repo.AddMessage(A.Fake<IMessage>(), A.Fake<EndPoint>(), 0));
		//}

		// TODO:  Validate the endpoint on the messages
		// TODO:  Validate the iterator of the messages
		// TODO:  Validate the RemoveAllFor of the messages
	}
}
