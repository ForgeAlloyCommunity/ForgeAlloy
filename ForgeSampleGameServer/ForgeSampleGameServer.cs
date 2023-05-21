using System;
using System.Text;
using Forge;
using Forge.Factory;
using Forge.ForgeAlloyUnity.Assets.ForgeNetworking.Utilities;
using Forge.Networking;
using Forge.Networking.Players;
using Forge.Networking.Sockets;
using Forge.Networking.Utilities;
using Forge.Serialization.Serializers;
using Forge.ServerRegistry.Messaging.Interpreters;
using ForgeSampleGameServer.Engine;
using ForgeSampleGameServer.Messaging.Interpreters;

namespace ForgeSampleGameServer
{
	public class ForgeSampleGameServer
	{

		/// <summary>
		/// This is the default name of the game server
		/// </summary>
		private const string defaultServerName = "Sample Game Server";

		private static ushort serverPort;
		private static string registryServerAddress;
		private static string serverName;

		public static string ServerName => serverName;

		internal static void Main(string[] args)
		{

			UserPrompts();

			StringBuilder consoleInput = new StringBuilder();
			ConsoleKeyInfo cki;

			// Create the synchronization context
			// ** We would normally use the Unity synchronisation context. But Because  **
			// ** this is a standalone console project, we need to create a context     **
			ForgeSynchronizationContext context = new ForgeSynchronizationContext();

			ForgeRegistration.Initialize();
			var factory = AbstractFactory.Get<INetworkTypeFactory>();
			factory.Register<IServerHolePunchInterpreter, ServerHolePunchInterpreter>();

			var serverMediator = factory.GetNew<INetworkMediator>();
			serverMediator.PlayerRepository.onPlayerAddedSubscription += OnPlayerAdded;
			serverMediator.PlayerRepository.onPlayerRemovedSubscription += OnPlayerRemoved;
			serverMediator.ChangeEngineProxy(new SampleGameServerEngine());
			serverMediator.StartServerWithRegistration(serverPort, 100, registryServerAddress, GlobalConst.defaultRegistryPort, serverName);
			Console.WriteLine("Server Started...");


			// Run Game Server on main thread with active synchronisation context
			context.Run((object obj) =>
			{
				serverMediator.SocketFacade.CancellationSource.Token.ThrowIfCancellationRequested();

				if (Console.KeyAvailable)
				{
					cki = Console.ReadKey(false);
					if (cki.Key == ConsoleKey.Enter)
					{
						string line = consoleInput.ToString().Trim();
						consoleInput.Clear();
						switch (line)
						{
							case "exit":
							case "quit":
								serverMediator.SocketFacade.CancellationSource.Cancel();
								break;
							case "post":
								context.Post((object state) => { Console.WriteLine("Post on main thread"); });
								break;
							case "list":
								Console.WriteLine("=== Begin Server List ===");
								var itr = serverMediator.PlayerRepository.GetEnumerator();
								while (itr.MoveNext())
								{
									var player = (INetPlayer)itr.Current;
									Console.WriteLine($"Server[{player.Id}] Name[{player.Name}]");
								}
								Console.WriteLine("=== End Server List ===");
								break;
							case "?":
								Console.WriteLine("Commands:");
								Console.WriteLine("exit/quit             Stop this service");
								Console.WriteLine("list                  List connected Players");
								Console.WriteLine("post                  Test execution of post");
								Console.WriteLine("");
								break;
							default:
								Console.WriteLine($"command {line} isn't supported yet");
								break;
						}
					}
					else
						consoleInput.Append(cki.KeyChar);
				}
			}, null, serverMediator.SocketFacade.CancellationSource);

		}

		private static void OnPlayerRemoved(INetPlayer player)
		{
			Console.WriteLine($"Player {player.Name}[{player.Id}] left game");
		}

		private static void OnPlayerAdded(INetPlayer player)
		{
			Console.WriteLine($"Player {player.Name}[{player.Id}] joined game");
		}

		private static void UserPrompts()
		{
			bool promptSuccess = false;

			while (!promptSuccess)
			{
				try
				{
					Console.WriteLine("Ctrl+C to quit");
					serverPort = SampleUtil.PromptInput<ushort>($"Enter ServerPort [default={GlobalConst.defaultServerPort}]", GlobalConst.defaultServerPort.ToString());
					registryServerAddress = SampleUtil.PromptInput<string>($"Enter Registry Server address [default={GlobalConst.loopBackAddress}]", GlobalConst.loopBackAddress);
					serverName = SampleUtil.PromptInput<string>($"Enter Server name [default={defaultServerName}]", defaultServerName);
					promptSuccess = true;
				}
				catch
				{
					Console.WriteLine("Input error, start again.");
				}

			}
		}


	}

}
