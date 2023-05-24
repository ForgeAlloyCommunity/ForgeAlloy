using Forge.Serialization;

namespace Forge.Networking.Messaging
{
	public delegate void MessageSent(IMessage message);

	public interface IMessage
	{
		event MessageSent OnMessageSent;

		/// <summary>
		/// A unique Id to identify reliable messages
		/// </summary>
		IMessageReceiptSignature Receipt { get; set; }

		/// <summary>
		/// The method to call to process this message
		/// </summary>
		IMessageInterpreter Interpreter { get; }

		/// <summary>
		/// Serial the message data into the buffer
		/// </summary>
		/// <param name="buffer"></param>
		void Serialize(BMSByte buffer);

		/// <summary>
		/// Deserialise the message data from the buffer
		/// </summary>
		/// <param name="buffer"></param>
		void Deserialize(BMSByte buffer);

		/// <summary>
		/// This method should be called to confirm the message has been sent
		/// This is defined on ForgeMessage and can be overriden on messages
		/// Calls OnMessageSent
		/// </summary>
		void Sent();

		/// <summary>
		/// This method is called when the message is removed from a message repository, eg: ForgeMessageRepository
		/// This is defined on ForgeMessage and can be overridden on messages
		/// </summary>
		void Unbuffered();

		/// <summary>
		/// This method is called when the message is removed from a waiting queue
		/// It calls Sent()
		/// </summary>
		void Dequeued();

		/// <summary>
		/// True if message has been returned to the Pool
		/// </summary>
		bool IsPooled { get; set; }

		/// <summary>
		/// IsBuffered is true when the message is stored in a message repository
		/// </summary>
		bool IsBuffered { get; set; }

		/// <summary>
		/// IsQueued is true if the message is stored in a queue for processing
		/// </summary>
		bool IsQueued { get; set; }

		/// <summary>
		/// IsSent is true once the message has been transmitted.
		/// IsSent can be true, while IsBuffered is also true. This happens when the message
		/// is a reliable message and the sender is waiting for an Ack
		/// </summary>
		bool IsSent { get; set; }

		/// <summary>
		/// IsSending is true when the message is in the process of being sent
		/// </summary>
		bool IsSending { get; set; }

		/// <summary>
		/// Called when message starts to be sent
		/// This is defined on ForgeMessage and can be overridden on messages
		/// </summary>
		void StartSending();

		/// <summary>
		/// Called when message has been sent
		/// This is defined on ForgeMessage and can be overridden on messages
		/// </summary>
		void FinishSending();

		/// <summary>
		/// Resets fields on the message.
		/// This should be defined on each message
		/// </summary>
		void Reset();
	}
}
