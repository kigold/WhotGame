using Orleans;
using Orleans.Runtime;
using WhotGame.Core.Enums;
using WhotGame.Abstractions.Extensions;
using WhotGame.Abstractions.GrainTypes;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Models.Requests;

namespace WhotGame.Grains
{
    public class GameGrain: Grain, IGameGrain
    {
        private readonly IPersistentState<GameState> _game;

        public GameGrain([PersistentState("Game", "WhotGame")] IPersistentState<GameState> game) 
        { 
            _game = game;
        }

        public override Task OnActivateAsync()
        {
            _game.State.Id = this.GetGrainIdentity().PrimaryKeyLong;
            return base.OnActivateAsync();
        }

        public Task<GameLite> GetGameAsync()
        {
            return Task.FromResult((GameLite)_game.State);
        }

        public async Task<PlayerGameScore[]> GetGameLeaderboardAsync()
        {
            var playersGameScore = new List<PlayerGameScore>();

            if (_game.State.Status == GameStatus.Ended)
            {
                foreach(var playerId in _game.State.PlayerIds)
                {
                    var player = GrainFactory.GetGrain<PlayerGrain>(playerId);
                    var cards = await player.GetGameCardsAsync(_game.State.Id);
                    playersGameScore.Add(new PlayerGameScore
                    {
                        PlayerId = playerId,
                        GameId = _game.State.Id,
                        TotalValue = cards.Sum(x => x.Value)
                    });
                }
            }

            return playersGameScore.ToArray();
        }

        public async Task<Card[]> GetPlayerGameCardsAsync(long playerId)
        {
            var player = GrainFactory.GetGrain<PlayerGrain>(playerId);
            return await player.GetGameCardsAsync(_game.State.Id);            
        }

        public async Task CreateGameAsync(long creatorId, CreateGameRequest request)
        {
            await StartGameAsync(creatorId, request.PlayerIds, request.IsPrivate, request.CardCount = 10);
        }

        public async Task<bool> StartGameAsync(long creatorId, long[] playerIds, bool isPrivate, int cardCount = 10)
        {
            //Init Game
            _game.State.CardCount = cardCount;
            _game.State.CreatorId = creatorId;
            _game.State.PlayerIds = new[] { creatorId }.Concat(playerIds).ToList();
            _game.State.CurrentPlayerTurnIndex = 0;


            var readyPlayers = new List<long>();
            if (isPrivate)
            {
                //Send invitation to players
                foreach (var playerId in playerIds)
                {
                    var player = GrainFactory.GetGrain<PlayerGrain>(playerId);
                    await player.SendGameInvitationsAsync(_game.State.Id, _game.State.CreatorId);
                }

                //wait for 40 secs
                var retry = 3;
                while (!readyPlayers.Any() && retry > 0)
                {
                    await Task.Delay(4000);
                    foreach (var playerId in playerIds)
                    {
                        var player = GrainFactory.GetGrain<PlayerGrain>(playerId);
                        var invitation = await player.GetGameInvitationAsync(_game.State.Id);

                        if (invitation.Response ?? false)
                            readyPlayers.Add(playerId);
                    }
                    retry--;
                }
            }
            else
            {
                await Task.Delay(36000); //wait for 120 seconds for people to join
            }

            //Generate Game Cards
            //Share Cards
            if (readyPlayers.Any())
            {
                _game.State.Cards = GenerateGameCards();
                ShareCards(_game.State.PlayerIds);
                _game.State.Status = GameStatus.Started;
            }
            else
            {
                _game.State.Status = GameStatus.Aborted;
            }

            return readyPlayers.Any();
        }

        public Task AddPlayerAsync(long playerId)
        {
            if (_game.State.Status == GameStatus.Created && !_game.State.PlayerIds.Contains(playerId))
                _game.State.PlayerIds.Add(playerId);

            return Task.CompletedTask;
        }

        public Task<List<Card>> TryPickCardsAsync(long playerId, int count = 1)
        {
            if (_game.State.PlayerIds[_game.State.CurrentPlayerTurnIndex] != playerId)
                return Task.FromResult(new List<Card>()); //It is not the selected players turn

            var player = GrainFactory.GetGrain<PlayerGrain>(playerId);
            var cards = new List<Card>();
            for(int i = 0; i < count; i++)
            {
                cards.Add(_game.State.Cards.Pop());
            }
            player.AddCardsAsync(_game.State.Id, cards);
            _game.State.GameLog.Add($"Player:{playerId} - {player.GetPlayerName} picked cards: {string.Join('|', cards.Select(x => $"{x.Id} - {x.Name}"))}");
            UpdatePlayerTurn();
            return Task.FromResult(cards);
        }

        public async Task TryPlayCard(long playerId, int cardId, CardColor? cardColor)
        {
            var player = GrainFactory.GetGrain<PlayerGrain>(playerId);

            var card = await player.TryPlayCardAsync(_game.State.Id, cardId, _game.State.LastPlayedCard);

            if (card == null)
                return;

            _game.State.LastPlayedCard = card;

            if (cardColor != null)
                _game.State.LastPlayedCard.Color = cardColor;


            _game.State.GameLog.Add($"Player:{playerId} - {player.GetPlayerName} played card:{card.Id} - {card.Name}");

            var cards = await player.GetGameCardsAsync(_game.State.Id);

            //Check if player has played all their cards
            if (!cards.Any())
            {
                EndGame();
                return;
            }

            UpdatePlayerTurn();
        }

        private void EndGame()
        {
            _game.State.Status = GameStatus.Ended;
            //Propergate message to FE that the game has ended
        }

        private void UpdatePlayerTurn()
        {
            var turnIncreament = _game.State.PlayerTurnReversed ? -1 : 1;

            _game.State.CurrentPlayerTurnIndex = (_game.State.CurrentPlayerTurnIndex + turnIncreament) % _game.State.PlayerIds.Count;
        }

        private void ShareCards(List<long> playerIds)
        {
            var playerCards = new Dictionary<long, List<Card>>();

            for(int i = 0; i < _game.State.CardCount; i++)
            {
                foreach (var playerId in playerIds)
                {
                    playerCards[playerId].Add(_game.State.Cards.Pop());
                }
            }

            foreach (var playerId in playerIds)
            {
                var player = GrainFactory.GetGrain<PlayerGrain>(playerId);
                player.SetGameCardsAsync(_game.State.Id, playerCards[playerId]);
            }
        }

        private static List<Card> GenerateGameCards()
        {
            var cards = new List<Card>();
            int count = 0;

            foreach(CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                foreach (CardShape shape in Enum.GetValues(typeof(CardShape)))
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        cards.Add(new Card
                        {
                            Id = count++,
                            Color = color,
                            Shape = shape,
                            IsSpecial = false,
                            Name = i.ToString(),
                            Value = i
                        });
                    }  
                    foreach(var special in SpecialCards)
                    {
                        cards.Add(new Card
                        {
                            Id = count++,
                            Color = color,
                            Shape = shape,
                            IsSpecial = false,
                            Name = special.Item1,
                            Value = special.Item2
                        });
                    }
                }
            }

            Random random = new Random();

            return cards.OrderBy(x => random.Next()).ToList();
        }

        private static (string, int)[] SpecialCards => new[] { ("joker", 50), ("pick-2", 50), ("pick-4", 50), ("general-market", 50), ("hold-on", 50), ("reverse", 50) };
    }
}