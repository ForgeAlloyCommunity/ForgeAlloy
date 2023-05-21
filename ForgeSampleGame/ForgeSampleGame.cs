using System;
using System.Net;
using System.Text;
using Forge;
using Forge.Factory;
using Forge.ForgeAlloyUnity.Assets.ForgeNetworking.Utilities;
using Forge.Networking;
using Forge.Networking.Messaging;
using Forge.Networking.Utilities;
using Forge.ServerRegistry.DataStructures;
using Forge.ServerRegistry.Messaging.Interpreters;
using Forge.ServerRegistry.Messaging.Messages;
using ForgeSampleGame.Engine;
using ForgeSampleGameServer.Engine;
using ForgeSampleGame.Messaging.Interpreters;
using System.Linq;

namespace ForgeSampleGame
{
	public class ForgeSampleGame
	{
		private static string registryServerAddress;
		private static SampleGameEngineFacade engine = new SampleGameEngineFacade();
		private static INetworkTypeFactory factory;
		private static EndPoint registryEndpoint;
		public static ServerListingEntry[] Servers { get; set; }

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
			factory.Register<IClientHolePunchInterpreter, ClientHolePunchInterpreter>();

			var registryEngine = new SampleRegistryEngineFacade();
			registryEngine.Connect(registryServerAddress, GlobalConst.defaultRegistryPort);
			Console.WriteLine("Client Started...");


			// Run Game Client on main thread with active synchronisation context
			context.Run((object obj) =>
			{
				registryEngine.CancellationSource.Token.ThrowIfCancellationRequested();

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
								registryEngine.CancellationSource.Cancel();
								break;
							case "list":
								RequestServerList(registryEngine.NetworkMediator);
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
			}, null, registryEngine.CancellationSource);

		}

		private static void Connect(string gameServerAddress)
		{
			string ip = "";
			ushort port = 0;

			var address = gameServerAddress.Split(':');
			if (address.Length > 1)
			{
				if (!ushort.TryParse(address[1], out port))
					Console.WriteLine($"Invalid port");
			}
			else
			{
				if (gameServerAddress == "x")
				{
					ip = "127.0.0.1";
					port = 12345;
				}
				else if (Servers == null)
				{
					Console.WriteLine("Invalid server reference");
				}
				else if (Servers.Any(s => s.Id.ToString() == gameServerAddress))
				{
					var entry = Servers.First(s => s.Id.ToString() == gameServerAddress);
					ip = entry.Address;
					port = entry.Port;
				}
			}

			if (port == 0) return;


			engine.Connect(ip, port, registryServerAddress, GlobalConst.defaultNatPort);


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
