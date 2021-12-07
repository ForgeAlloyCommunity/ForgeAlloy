using Forge.Engine;

namespace ForgeServerRegistryService.Engine
{
	public class ServerRegistryEngine : IEngineProxy
	{
		public IForgeLogger Logger => throw new System.NotImplementedException();

		public void NetworkingEstablished()
		{
			// Not expecting to have any entities on the server registry engine
			return;
		}
	}
}
