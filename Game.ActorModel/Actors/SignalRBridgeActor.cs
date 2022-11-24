using Akka.Actor;
using Game.ActorModel.ExternalSystems;
using Game.ActorModel.Messages;

namespace Game.ActorModel.Actors
{
	public class SignalRBridgeActor : ReceiveActor
	{
		private readonly IGameEventsPusher _gameEventPusher;
		private readonly IActorRef _gameController;

		public SignalRBridgeActor(IGameEventsPusher gameEventPusher, IActorRef gameController)
		{
			_gameEventPusher = gameEventPusher;
			_gameController = gameController;

			Receive<JoinGameMessage>(message => JoinGame(message));

			Receive<AttackPlayerMessage>(message => AttackPlayerMessage(message));

			Receive<PlayerStatusMessage>(message => PlayerJoined(message));

			Receive<PlayerHealthChangedMessage>(message => PlayerHealthChanged(message));
		}

        private void AttackPlayerMessage(AttackPlayerMessage message)
        {
            _gameController.Tell(message);
        }

        private void JoinGame(JoinGameMessage message)
        {
            _gameController.Tell(message);
        }

        private void PlayerHealthChanged(PlayerHealthChangedMessage message)
        {
            _gameEventPusher.UpdatePlayerHealth(message.PlayerName, message.Health);
        }

        private void PlayerJoined(PlayerStatusMessage message)
        {
			_gameEventPusher.PlayerJoined(message.PlayerName, message.Health);
        }
    }
}

