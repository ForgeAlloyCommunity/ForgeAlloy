﻿using Forge.Engine;

namespace ForgeServerRegistryService.Engine
{
	public class ServerRegistryEngine : IEngineProxy
	{
		private IForgeLogger logger = new ForgeConsoleLogger();
		public IForgeLogger Logger => logger;

		public void NetworkingEstablished()
		{
			// Not expecting to have any entities on the server registry engine
			return;
		}
	}
}
