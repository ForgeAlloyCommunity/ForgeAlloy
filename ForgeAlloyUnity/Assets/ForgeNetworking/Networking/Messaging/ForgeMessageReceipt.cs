using Forge.DataStructures;

namespace Forge.Networking.Messaging
{
	public class ForgeMessageReceipt : ForgeSignature, IMessageReceiptSignature
	{
		public ForgeMessageReceipt(ISignatureGenerator<int> generator)
			: base(generator)
		{

		}

		public void Init(ISignatureGenerator<int> generator)
		{
			_id = generator.Generate();
		}
	}
}
