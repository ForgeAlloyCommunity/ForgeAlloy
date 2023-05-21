using System.Data.Common;
using Forge.Engine;

namespace ForgeServerRegistryService.Engine
{
	public class NatEngine : IEngineProxy
	{
		private IForgeLogger logger = new ForgeConsoleLogger();
		public IForgeLogger Logger => logger;

		public string Id => "Nat";

		ServerRegistryEngine _serverEngine;
		public ServerRegistryEngine ServerEngine => _serverEngine;

		public NatEngine(ServerRegistryEngine serverEngine)
		{
			_serverEngine= serverEngine;
		}

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
