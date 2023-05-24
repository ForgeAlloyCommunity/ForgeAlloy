using System;
using System.Text;
using Forge;
using Forge.Factory;
using Forge.Utilities;
using Forge.Networking;
using Forge.Networking.Players;
using Forge.Networking.Sockets;
using Forge.ServerRegistry.Messaging.Interpreters;
using ForgeServerRegistryService.Engine;
using ForgeServerRegistryService.Messaging.Interpreters;
using ForgeServerRegistryService.Networking.Players;

namespace ForgeServerRegistryService
{
	public class ForgeServerRegistryService
	{

		/// <summary>
		/// This is the default max players. That is, the maximum number of servers that can be registered
		/// </summary>
		private const ushort defaultMaxPlayers = 1000;

		/// <summary>
		/// Are we running in Docker
		/// </summary>
		private static bool InDocker { get { return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"; } }

		private static ForgeSynchronizationContext context;
		private static INetworkMediator networkMediator;

		private static INetworkMediator natMediator;
		private static void Main(string[] args)
		{



			// Create the synchronization context
			// (Note: Use Unity syncrhonisation default context if using this code within a Unity project)
			context = new ForgeSynchronizationContext();

			ForgeRegistration.Initialize();
			var factory = AbstractFactory.Get<INetworkTypeFactory>();
			factory.Register<IRegisterAsServerInterpreter, RegisterAsServerInterpreter>();
			factory.Register<IGetServerRegistryInterpreter, GetServerRegistryInterpreter>();
			factory.Register<IConnectServerRegistryInterpreter, ConnectServerRegistryInterpreter>();
			factory.Replace<INetPlayer, RegisteredServer>();
			factory.Replace<ISocketNatFacade, ForgeUDPNatFacade>();

			networkMediator = factory.GetNew<INetworkMediator>();
			networkMediator.PlayerRepository.onPlayerAddedSubscription += onPlayerAdded;
			networkMediator.PlayerRepository.onPlayerRemovedSubscription += onPlayerDisconnected;
			var engine = new ServerRegistryEngine();
			networkMediator.ChangeEngineProxy(engine);
			engine.NetContainer = networkMediator;
			networkMediator.StartServer(GlobalConst.defaultRegistryPort, defaultMaxPlayers);

			natMediator = factory.GetNew<INetworkMediator>();
			natMediator.ChangeEngineProxy(new NatEngine((ServerRegistryEngine)networkMediator.EngineProxy));
			natMediator.StartNatServer(GlobalConst.defaultNatPort);

			Console.WriteLine($"Registry Server listening on port {GlobalConst.defaultRegistryPort}");


			if (InDocker)
				RunInDocker();
			else
				RunInConsole();

		}


		private static void RunInDocker()
		{

			try
			{
				// Run Registry Server on main thread with active synchronisation context
				context.Run((object obj) =>
				{
					networkMediator.SocketFacade.CancellationSource.Token.ThrowIfCancellationRequested();

				}, null, networkMediator.SocketFacade.CancellationSource);
			}
			catch (OperationCanceledException)
			{
				networkMediator.EngineProxy.Logger.Log("Cancelling the background network read task");

			}
		}

		private static void RunInConsole()
		{
			StringBuilder consoleInput = new StringBuilder();
			ConsoleKeyInfo cki;

			try
			{
				// Run Registry Server on main thread with active synchronisation context
				context.Run((object obj) =>
				{
					networkMediator.SocketFacade.CancellationSource.Token.ThrowIfCancellationRequested();

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
									networkMediator.SocketFacade.CancellationSource.Cancel();
									break;
								case "list":
									Console.WriteLine("=== Begin Server List ===");
									var itr = networkMediator.PlayerRepository.GetEnumerator();
									while (itr.MoveNext())
									{
										var server = (RegisteredServer)itr.Current;
										if (server.IsRegisteredServer)
											Console.WriteLine($"Server[{server.Id}] Name[{server.Name}] Players[{server.CurrentPlayers}/{server.MaxPlayers}]");
									}
									Console.WriteLine("=== End Server List ===");
									break;
								case "?":
									Console.WriteLine("Commands:");
									Console.WriteLine("exit/quit             Stop this service");
									Console.WriteLine("list                  List active servers");
									Console.WriteLine("");
									break;
								default:
									Console.WriteLine($"This command isn't supported yet");
									break;
							}
						}
						else
							consoleInput.Append(cki.KeyChar);
					}
				}, null, networkMediator.SocketFacade.CancellationSource);
			}
			catch (OperationCanceledException)
			{
				networkMediator.EngineProxy.Logger.Log("Cancelling the background network read task");

			}
		}

		private static void onPlayerDisconnected(INetPlayer server)
		{
			Console.WriteLine($"EndPoint Disconnected: {server.Id}");
		}

		private static void onPlayerAdded(INetPlayer player)
		{
			Console.WriteLine($"EndPoint Connected: {player.Id}");
		}
	}
}
