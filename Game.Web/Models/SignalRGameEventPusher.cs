using System;
using Game.ActorModel.ExternalSystems;
using Microsoft.AspNetCore.SignalR;

namespace Game.Web.Models
{
	public class SignalRGameEventPusher : IGameEventsPusher
	{
        private readonly IHubContext<GameHub> _gameHubContext;

		public SignalRGameEventPusher(IHubContext<GameHub> hub)
		{
            _gameHubContext = hub;
		}

        public void PlayerJoined(string playerName, int playerHealth)
        {
            _gameHubContext.Clients.All.SendAsync("playerJoined", playerName, playerHealth);
        }

        public void UpdatePlayerHealth(string playerName, int playerHealth)
        {
            _gameHubContext.Clients.All.SendAsync("updatePlayerHealth", playerName, playerHealth);
        }
    }
}

