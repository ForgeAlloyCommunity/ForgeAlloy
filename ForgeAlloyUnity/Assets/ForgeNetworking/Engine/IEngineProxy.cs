namespace Forge.Engine
{
	public interface IEngineProxy
	{
		IForgeLogger Logger { get; }
		void NetworkingEstablished();

		/// <summary>
		/// This is called whenever a Connection handshake is received
		/// This should always return true the first time the connection challenge is received
		/// If this is received a second time, then the client has disconnected and this is a
		/// reconnect challenge. The game server, will create a new INetPlayer if this happens.
		/// Most projects require a stateful connection and so this would result in a client timeout.
		/// But for stateless client/server connections, the connection should be allowed to reconnect
		/// An example of a stateless connection is the client/regestry service connection
		/// </summary>
		/// <returns></returns>
		bool CanConnectToChallenge();

		string Id { get; }
	}
}
