using System;
using Forge.Engine;
using Forge.Factory;
using Forge.Networking;
using Forge.Networking.Players;
using Forge.Networking.Sockets;

namespace ForgeSampleGame.Engine
{
	internal class SampleGameEngineFacade : IEngineProxy
	{
		private IForgeLogger m_logger = new ForgeConsoleLogger();
		public IForgeLogger Logger => m_logger;
		public string Id => "Client";

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


		public void Connect(string ip, ushort port, string registryAddress, ushort natPort)
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
				NetworkMediator.StartClientWithNat(ip, port, registryAddress, natPort);
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		public bool CanConnectToChallenge()
		{
			// We are already connected. If we receive another challenge
			// then we must have disconnected and reconnected.
			if (IsConnected)
			{
				ClientTimeout();
				return false;
			}
			return true;
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

		private void ClientTimeout()
		{
			this.IsConnected = false;
			this.IsConnecting = false;
			Console.WriteLine($"Client Timeout. MyPlayerId[{MyPlayerId}]");
		}

	}
}
