using Forge.Engine;


namespace ForgeSampleGameServer.Engine
{
	public  class SampleRegistryEngine : IEngineProxy
	{
		private IForgeLogger logger = new ForgeConsoleLogger();
		public IForgeLogger Logger => logger;
		public string Id => "SampleRegistry";

		public bool CanConnectToChallenge()
		{
			return true;
		}

		public void NetworkingEstablished()
		{
			// Not expecting to have any entities on the server registry engine
			return;
		}
	}
}
