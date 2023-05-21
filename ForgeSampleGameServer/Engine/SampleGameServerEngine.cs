using System;
using Forge.Engine;

namespace ForgeSampleGameServer.Engine
{
	internal class SampleGameServerEngine : IEngineProxy
	{
		private IForgeLogger logger = new ForgeConsoleLogger();
		public IForgeLogger Logger => logger;
		public string Id => "Game";

		public bool CanConnectToChallenge()
		{
			return false;
		}

		public void NetworkingEstablished()
		{
			// Not expecting to have any entities on the server registry engine
			Console.WriteLine("Server Networking Established");
			return;
		}
	}
}
