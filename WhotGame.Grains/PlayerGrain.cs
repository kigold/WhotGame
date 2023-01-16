using Orleans;
using Orleans.Runtime;
using WhotGame.Abstractions.Extensions;
using WhotGame.Abstractions.GrainTypes;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Models.Requests;

namespace WhotGame.Grains
{
    public class PlayerGrain : Grain, IPlayerGrain
    {
        private readonly IPersistentState<PlayerState> _player;
        public PlayerGrain([PersistentState("Player", "WhotGame")] IPersistentState<PlayerState> player)
        {
            _player = player;
        }

        public Task<string> GetPlayerName()
        {
            return Task.FromResult(_player.State.Username);
        }
        public Task<GameInvitation[]> GetGameInvitationsAsync()
        {
            return Task.FromResult(_player.State.Invitations.Select(x => x.Value).ToArray());
        }

        public Task<GameInvitation> GetGameInvitationAsync(long gameId)
        {
            return Task.FromResult(_player.State.Invitations[gameId]);
        }

        public Task<GameLite[]> GetGamesAsync()
        {
            return Task.FromResult(_player.State.Games.Select(x => x.Value).ToArray());
        }

        public Task<Card[]> GetGameCardsAsync(long gameId)
        {
            return Task.FromResult(_player.State.GameCards[gameId].ToArray());
        }

        public Task SetGameCardsAsync(long gameId, List<Card> cards)
        {
            _player.State.GameCards[gameId] = cards;

            return Task.CompletedTask;
        }

        public Task AddCardsAsync(long gameId, List<Card> cards)
        {
            _player.State.GameCards[gameId].Concat(cards);

            return Task.CompletedTask;
        }

        public Task SendGameInvitationsAsync(long gameId, long playerId)
        {
            //Send Invitation Notification
            _player.State.Invitations[gameId] = new GameInvitation
            {
                GameId = gameId,
                PlayerId = playerId
            };

            return Task.CompletedTask;
        }

        public async Task RespondToGameInvitationsAsync(long gameId, bool response)
        {
            var invitation = _player.State.Invitations[gameId];

            if (invitation.Response == null)
                invitation.Response = response;

            var gameGrain = GrainFactory.GetGrain<GameGrain>(gameId);

            if (response)
                _player.State.Games[gameId] = await gameGrain.GetGameAsync();
        }

        public Task<Card> TryPlayCardAsync(long gameId, int cardId, Card cardToMatch)
        {
            var card = _player.State.GameCards[gameId].FirstOrDefault(x => x.Id == cardId);

            if (!ValidateCard(card, cardToMatch))
                return Task.FromResult((Card)null);

            return Task.FromResult(_player.State.GameCards[gameId].Pop());
        }

        private bool ValidateCard(Card card, Card cardToMatch)
        {
            return cardToMatch.Color == card.Color ||
                cardToMatch.Shape == card.Shape ||
                string.Equals(cardToMatch.Name, card.Name);
        }
    }
}
