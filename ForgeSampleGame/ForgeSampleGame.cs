using System;
using System.Text;
using Forge;
using Forge.Factory;
using Forge.ForgeAlloyUnity.Assets.ForgeNetworking.Utilities;
using Forge.Networking;
using Forge.Networking.Messaging;
using Forge.Networking.Messaging.Messages;
using Forge.Networking.Utilities;
using Forge.ServerRegistry.Messaging.Interpreters;
using Forge.ServerRegistry.Messaging.Messages;
using ForgeSampleGame.Engine;
using ForgeServerRegistryService.Messaging.Interpreters;

namespace ForgeSampleGame
{
	public class ForgeSampleGame
	{
		private static string registryServerAddress;
		private static SampleGameEngine engine = new SampleGameEngine();
		private static INetworkTypeFactory factory;

		private static void Main(string[] args)
		{

			UserPrompts();

			StringBuilder consoleInput = new StringBuilder();
			ConsoleKeyInfo cki;

			// Create the synchronization context
			// ** We would normally use the Unity synchronisation context. But Because  **
			// ** this is a standalone console project, we need to create a context     **
			// To post on main thread from another thread:
			// context.Post((object state) => { Console.WriteLine("Post on main thread"); });
			ForgeSynchronizationContext context = new ForgeSynchronizationContext();

			ForgeRegistration.Initialize();
			factory = AbstractFactory.Get<INetworkTypeFactory>();
			factory.Register<IServerRegistryInterpreter, ForgeServerRegistryInterpreter>();

			var registryMediator = factory.GetNew<INetworkMediator>();
			registryMediator.ChangeEngineProxy(new SampleGameEngine());
			registryMediator.StartClient(registryServerAddress, GlobalConst.defaultRegistryPort);
			Console.WriteLine("Client Started...");


			// Run Game Client on main thread with active synchronisation context
			context.Run((object obj) =>
			{
				registryMediator.SocketFacade.CancellationSource.Token.ThrowIfCancellationRequested();

				if (Console.KeyAvailable)
				{
					cki = Console.ReadKey(false);
					if (cki.Key == ConsoleKey.Enter)
					{
						string line = consoleInput.ToString().Trim();
						Console.WriteLine(line);
						consoleInput.Clear();
						switch (line.Split(' ')[0])
						{
							case "exit":
							case "quit":
								registryMediator.SocketFacade.CancellationSource.Cancel();
								break;
							case "list":
								RequestServerList(registryMediator);
								break;
							case "connect":
								if (line.Split(' ').Length > 1)
									Connect(line.Split(' ')[1]);
								else
									Console.WriteLine("Game server address not entered");
								break;
							case "?":
								Console.WriteLine("Commands:");
								Console.WriteLine("exit/quit             Stop this service");
								Console.WriteLine("list                  List available servers");
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
			}, null, registryMediator.SocketFacade.CancellationSource);

		}

		private static void Connect(string gameServerAddress)
		{
			var address = gameServerAddress.Split(':');
			if (address.Length < 1)
			{
				Console.WriteLine($"Invalid game server address");
				return;
			}

			if (ushort.TryParse(address[1], out ushort port))
			{
				engine.Connect(address[0], port);
			}
			else
				Console.WriteLine("Invalid port");

		}

		private static void RequestServerList(INetworkMediator mediator)
		{
			var msg = ForgeMessageCodes.Instantiate<ForgeGetServerRegistryMessage>();
			mediator.SendMessage(msg);
		}

		private static void UserPrompts()
		{
			bool promptSuccess = false;

			while (!promptSuccess)
			{
				try
				{
					Console.WriteLine("Ctrl+C to quit");
					registryServerAddress = SampleUtil.PromptInput<string>($"Enter Registry Server address [default={GlobalConst.loopBackAddress}]", GlobalConst.loopBackAddress);
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
