using System;
using System.Threading;
using Forge.Engine;
using Forge.Factory;
using Forge.Networking;
using Forge.Networking.Sockets;


namespace ForgeSampleGameServer.Engine
{
	public  class SampleRegistryEngineFacade : IEngineProxy
	{
		private IForgeLogger logger = new ForgeConsoleLogger();
		public IForgeLogger Logger => logger;

		public INetworkMediator NetworkMediator { get; set; }
		private ISocketFacade _selfSocket => NetworkMediator.SocketFacade;

		public CancellationTokenSource CancellationSource => NetworkMediator.SocketFacade.CancellationSource;

		public bool IsConnecting { get; set; }
		public bool IsConnected { get; set; }

		public void Connect(string ip, ushort port)
		{
			if (NetworkMediator != null)
			{
				Console.WriteLine("Already connected");
				return;
			}

			var factory = AbstractFactory.Get<INetworkTypeFactory>();
			NetworkMediator = factory.GetNew<INetworkMediator>();

			NetworkMediator.ChangeEngineProxy(this);

			this.IsConnecting = true;
			this.IsConnected = false;

			try
			{
				Console.WriteLine($"Connecting {ip}:{port}");
				NetworkMediator.StartClient(ip, port);
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		public void NetworkingEstablished()
		{
			if (IsConnecting)
			{
				Console.WriteLine($"Connected to Registry");
				IsConnected = true;
				IsConnecting = false;
			}
			return;
		}

	}
}
