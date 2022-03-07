using Forge.DataStructures;

namespace Forge.Networking.Messaging
{
	public interface IMessageReceiptSignature : ISignature
	{
		void Init(ISignatureGenerator<int> generator);
	}
}
