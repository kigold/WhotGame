using Orleans;
using Orleans.Runtime;
using WhotGame.Abstractions.Extensions;
using WhotGame.Abstractions.GrainTypes;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Data.Models;
using WhotGame.Core.Data.Repositories;

namespace WhotGame.Grains
{
    public class PlayerGrain : Grain, IPlayerGrain
    {
        private readonly IPersistentState<PlayerState> _player;
        private readonly IRepository<User> _userRepo;
        public PlayerGrain([PersistentState("Player", "WhotGame")] IPersistentState<PlayerState> player, IRepository<User> userRepo)
        {
            _player = player;
            _userRepo = userRepo;
        }

        public override Task OnActivateAsync()
        {
            var user = _userRepo.GetByID(this.GetGrainIdentity().PrimaryKeyLong);
            _player.State.Id = user.Id;
            _player.State.Email = user.Email;
            _player.State.FullName = user.FullName;
            _player.State.Username = user.UserName;
            return base.OnActivateAsync();
        }

        public Task<PlayerLite> GetPlayerAsync()
        {
            return Task.FromResult( (PlayerLite)_player.State );
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
            if (_player.State.GameCards.ContainsKey(gameId))
                return Task.FromResult(_player.State.GameCards[gameId].ToArray());
            return Task.FromResult(new Card[0]);
        }

        public Task SetGameCardsAsync(long gameId, List<Card> cards)
        {
            _player.State.GameCards[gameId] = cards;

            return Task.CompletedTask;
        }

        public Task AddCardsAsync(long gameId, List<Card> cards)
        {
            cards.ForEach(x => _player.State.GameCards[gameId].Add(x));

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
                _player.State.Games[gameId] = await gameGrain.GetGamesAsync();
        }

        public Task<Card> TryPlayCardAsync(long gameId, int cardId, Card cardToMatch)
        {
            var card = _player.State.GameCards[gameId].FirstOrDefault(x => x.Id == cardId);

            if (!ValidateCard(card, cardToMatch))
                return Task.FromResult((Card)null);

            return Task.FromResult(_player.State.GameCards[gameId].Pop(card));
        }

        private bool ValidateCard(Card card, Card cardToMatch)
        {
            return cardToMatch.Color == card.Color ||
                cardToMatch.Shape == card.Shape ||
                string.Equals(cardToMatch.Name, card.Name);
        }
    }
}
