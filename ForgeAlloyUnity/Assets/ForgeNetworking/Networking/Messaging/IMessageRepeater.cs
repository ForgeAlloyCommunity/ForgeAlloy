using System.Net;

namespace Forge.Networking.Messaging
{
	public interface IMessageRepeater
	{
		int RepeatMillisecondsInterval { get; set; }
		void Start(INetworkMediator networkMediator);
		void AddMessageToRepeat(IMessage message, EndPoint receiver, int ttlMilliseconds = 0);
		void RemoveRepeatingMessage(EndPoint sender, IMessageReceiptSignature messageReceipt, ushort recentPackets);
		void RemoveAllFor(EndPoint receiver);
		double GetTTLDiff(EndPoint sender, IMessageReceiptSignature messageReceipt, int ttlMilliseconds);
		int GetRepeatBufferCount();
		IMessageReceiptSignature GetNewMessageReceipt(EndPoint receiver);
	}
}
