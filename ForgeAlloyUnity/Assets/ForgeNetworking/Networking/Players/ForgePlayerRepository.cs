using System;
using System.Collections.Generic;
using System.Net;
using Forge.Factory;
using Forge.Utilities;

namespace Forge.Networking.Players
{
	public class ForgePlayerRepository : IPlayerRepository
	{
		private readonly Dictionary<IPlayerSignature, INetPlayer> _playerLookup = new Dictionary<IPlayerSignature, INetPlayer>();
		private readonly Dictionary<long, INetPlayer> _playerAddressLookup = new Dictionary<long, INetPlayer>();

		public event PlayerAddedToRepository onPlayerAddedSubscription;
		public event PlayerAddedToRepository onPlayerRemovedSubscription;

		public int TimeoutMilliseconds { get; set; } = 10000;
		public int Count { get => _playerLookup.Count; }

		public void AddPlayer(INetPlayer player)
		{
			if (_playerAddressLookup.ContainsKey(player.EndPoint.IPKey()))
			{
				throw new PlayerAlreadyExistsException(player.EndPoint);
			}
			if (player.Id == null)
				player.Id = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IPlayerSignature>();
			_playerLookup.Add(player.Id, player);
			_playerAddressLookup.Add(player.EndPoint.IPKey(), player);
			onPlayerAddedSubscription?.Invoke(player);
		}

		public INetPlayer GetPlayer(IPlayerSignature id)
		{
			if (!_playerLookup.TryGetValue(id, out var player))
				throw new PlayerNotFoundException(id);
			return player;
		}

		public void RemovePlayer(INetPlayer player)
		{
			if (!_playerLookup.ContainsKey(player.Id))
				throw new PlayerNotFoundException(player.Id);

			if (!_playerAddressLookup.ContainsKey(player.EndPoint.IPKey()))
				throw new PlayerNotFoundException(player.EndPoint);

			_playerLookup.Remove(player.Id);
			_playerAddressLookup.Remove(player.EndPoint.IPKey());
			onPlayerRemovedSubscription?.Invoke(player);
		}

		public void RemovePlayer(IPlayerSignature id)
		{
			if (!_playerLookup.ContainsKey(id))
				throw new PlayerNotFoundException(id);

			RemovePlayer(_playerLookup[id]);
		}

		public INetPlayer GetPlayer(EndPoint endpoint)
		{
			if (!_playerAddressLookup.TryGetValue(endpoint.IPKey(), out var player))
				throw new PlayerNotFoundException(endpoint);
			return player;
		}

		public bool Exists(EndPoint endpoint)
		{
			return _playerAddressLookup.ContainsKey(endpoint.IPKey());
			//return _playerAddressLookup.TryGetValue(endpoint.GetHashCode(), out _);
		}

		public bool Exists(IPlayerSignature id)
		{
			return _playerLookup.ContainsKey(id);
			//return _playerLookup.TryGetValue(id, out _);
		}

		public IEnumerator<INetPlayer> GetEnumerator()
		{
			return _playerLookup.Values.GetEnumerator();
		}
	}
}
