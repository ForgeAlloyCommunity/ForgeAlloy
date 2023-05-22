using System.Net;

namespace Forge.Networking.Messaging
{
	public delegate void MessageRepositoryIterator(EndPoint endpoint, IMessage message);

	public interface IMessageRepository
	{
		void AddMessage(IMessage message, EndPoint sender);
		void AddMessage(IMessage message, EndPoint sender, int ttlMilliseconds);
		void RemoveAllFor(EndPoint sender);
		void RemoveMessage(EndPoint sender, IMessage message);
		void RemoveMessage(EndPoint sender, IMessageReceiptSignature receipt);
		void RemoveMessage(EndPoint sender, IMessageReceiptSignature receipt, ushort recentPackets);
		bool Exists(EndPoint sender, IMessageReceiptSignature receipt);
		void Iterate(MessageRepositoryIterator iterator);
		void Clear();
		ushort ProcessReliableSignature(EndPoint sender, int id);
		IMessageReceiptSignature GetNewMessageReceipt(EndPoint receiver);

		/// <summary>
		/// Keep track of repository purpose
		/// </summary>
		string Id { get; set; }
	}
}
