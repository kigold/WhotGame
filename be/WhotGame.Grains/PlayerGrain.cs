using Orleans;
using Orleans.Runtime;
using System.ComponentModel.DataAnnotations;
using WhoteGame.Services;
using WhotGame.Abstractions.Extensions;
using WhotGame.Abstractions.GrainTypes;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Data.Models;
using WhotGame.Core.Data.Repositories;
using WhotGame.Core.Enums;
using static WhotGame.Abstractions.Constants;

namespace WhotGame.Grains
{
    public class PlayerGrain : Grain, IPlayerGrain
    {
        private readonly IPersistentState<PlayerState> _player;
        private readonly IRepository<User> _userRepo;
        private readonly IGameService _gameService;
        public PlayerGrain([PersistentState("Player", "WhotGame")] IPersistentState<PlayerState> player, IRepository<User> userRepo, IGameService gameService)
        {
            _player = player;
            _userRepo = userRepo;
            _gameService = gameService;
        }

        public override Task OnActivateAsync()
        {
            var user = _userRepo.GetByID(this.GetGrainIdentity().PrimaryKeyLong);
            _player.State.Id = user.Id;
            _player.State.Email = user.Email;
            _player.State.FullName = user.FullName;
            _player.State.Name = user.Firstname;
            _player.State.Avatar = user.Avatar;
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

        public Task<Card> TryPlayCardAsync(long gameId, int cardId, CardColor? color, CardShape? shape, Card cardToMatch, bool hasPendingPick2, bool hasPendingPick4)
        {
            var card = _player.State.GameCards[gameId].First(x => x.Id == cardId);

            if (hasPendingPick2 || hasPendingPick4) // if player received pick2 or pick4, they are axpected to reply accordingly or pick the cards
            {
                if (hasPendingPick2 && !string.Equals(card.Name, PICK2))
                    throw new ValidationException("Pick 2 or reply by playing Pick2");

                if (hasPendingPick4 && !string.Equals(card.Name, PICK4))
                    throw new ValidationException("Pick 4 or reply by playing Pick4");
            }

            if (string.Equals(card.Name, JOKER) && (color == null || shape == null))
                throw new ValidationException("Card Color and Card Shape is required for Joker Card");

            if (!ValidateCard(card, cardToMatch))
                throw new ValidationException("Card does not tally with last played card");

            return Task.FromResult(_player.State.GameCards[gameId].Pop(card));
        }

        private bool ValidateCard(Card card, Card cardToMatch)
        {
            return cardToMatch.Color == card.Color ||
                cardToMatch.Shape == card.Shape ||
                string.Equals(cardToMatch.Name, card.Name) || (card.IsSpecial && string.Equals(card.Name, JOKER));
        }

        public async Task AddPlayerToGame(long gameId)
        {
            var gameGrain = GrainFactory.GetGrain<IGameGrain>(gameId);
            _player.State.Games[gameId] = await gameGrain.GetGamesAsync();

            //Add Player to Player Game Repo
            //This should be revised if we can persist player game using orleans state
            await _gameService.AddPlayerToGame(_player.State.Id, gameId);
        }

        public async Task EndGame(long gameId)
        {
            _player.State.Games.TryGetValue(gameId, out var game);
            if (game != null)
            {
                game.Status = GameStatus.Ended;
            }

            await _gameService.RemovePlayerFromGame(_player.State.Id, gameId);
        }
    }
}
