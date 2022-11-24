using System;
using Game.ActorModel.ExternalSystems;
using Microsoft.AspNetCore.SignalR;

namespace Game.Web.Models
{
	public class SignalRGameEventPusher : IGameEventsPusher
	{
        private readonly IHubContext _gameHubContext;

		public SignalRGameEventPusher()
		{
            //_gameHubContext = hubContext;
		}

        public void PlayerJoined(string playerName, int playerHealth)
        {
            // _gameHubContext.Clients.All.playerJoined(playerName, playerHealth);
        }

        public void UpdatePlayerHealth(string playerName, int playerHealth)
        {
            // _gameHubContext.Clients.All.updatePlayerHealth(playerName, playerHealth);
        }
    }
}

