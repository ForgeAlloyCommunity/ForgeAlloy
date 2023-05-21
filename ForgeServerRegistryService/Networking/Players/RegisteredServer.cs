using System;
using System.Net;
using Forge.Networking.Players;

namespace ForgeServerRegistryService.Networking.Players
{
	public class RegisteredServer : INetPlayer
	{
		private EndPoint _endPoint;
		private string _ip;
		private ushort _port;

		public EndPoint EndPoint
		{
			get	{ return _endPoint;	}
			set
			{
				_endPoint = value;
				if (_endPoint is IPEndPoint)
				{
					_ip = ((IPEndPoint) _endPoint).Address.MapToIPv4().ToString();
					_port = (ushort)((IPEndPoint) _endPoint).Port;
				}
			}
		}
		public IPlayerSignature Id { get; set; }
		public string Name { get; set; }
		public bool IsInEngine { get; set; }
		public bool IsRegisteredServer { get; set; }
		public DateTime LastCommunication { get; set; }
		public int MaxPlayers { get; set; }
		public int CurrentPlayers { get; set; }
		public string IP { get { return _ip; } }
		public ushort Port { get { return _port;} }
	}
}
