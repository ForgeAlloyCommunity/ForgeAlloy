using System;
using Forge.Engine;
using Forge.Factory;
using Forge.Networking;
using Forge.Networking.Players;
using Forge.Networking.Sockets;

namespace ForgeSampleGame.Engine
{
	internal class SampleGameEngine : IEngineProxy
	{
		private IForgeLogger m_logger = new ForgeConsoleLogger();
		public IForgeLogger Logger => m_logger;

		public INetworkMediator NetworkMediator { get; set; }
		private ISocketFacade _selfSocket => NetworkMediator.SocketFacade;

		public IPlayerSignature MyPlayerId
		{
			get
			{
				if (NetworkMediator != null)
				{
					if (NetworkMediator.SocketFacade != null)
					{
						return NetworkMediator.SocketFacade.NetPlayerId;
					}
					else
						return null;
				}
				else
					return null;

			}
		}

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
				ClientStarted();
				IsConnected= true;
				IsConnecting= false;
			}
			return;
		}

		private void ClientStarted()
		{
			Console.WriteLine($"Client Started. MyPlayerId[{MyPlayerId}]");
		}

	}
}
