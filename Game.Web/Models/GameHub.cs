using System;
using Akka.Actor;
using Game.ActorModel.Actors;
using Game.ActorModel.Messages;
using Microsoft.AspNetCore.SignalR;

namespace Game.Web.Models
{
	public class GameHub : Hub
	{
		private readonly SignalRBridgeActor _signalRBridge;

		public GameHub(SignalRBridgeActor signalRBridgeActor)
		{
			_signalRBridge = signalRBridgeActor;
		}

		public void JoinGame(string playerName)
		{
			GameActorSystem.ActorReferences.SignalRBridge.Tell(new JoinGameMessage(playerName));
		}

		public void Attack(string playerName)
		{
			GameActorSystem.ActorReferences.SignalRBridge.Tell(new AttackPlayerMessage(playerName));
		}
	}
}

