using Orleans;
using Orleans.Runtime;
using WhotGame.Abstractions.Extensions;
using WhotGame.Abstractions.GrainTypes;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Enums;
using WhotGame.Core.Models.Requests;
using WhotGame.Core.Exceptions;

namespace WhotGame.Grains
{
    public class GameGrain: Grain, IGameGrain
    {
        private readonly IPersistentState<GameState> _game;
        private List<PlayerLite> _players = new List<PlayerLite>();
        private int _gameRetry = 3;
        private IDisposable _timer;
        private DateTime _lastActivityTime;
        private List<(long, int, string)> _marketPickerCount = new List<(long, int, string)> ();

        public GameGrain([PersistentState("Game", "WhotGame")] IPersistentState<GameState> game) 
        { 
            _game = game;
        }

        public override Task OnActivateAsync()
        {
            _game.State.Id = this.GetGrainIdentity().PrimaryKeyLong;
            return base.OnActivateAsync();
        }

        public Task<GameLite> GetGamesAsync()
        {
            return Task.FromResult((GameLite)_game.State);
        }

        public async Task<GameStats> GetGameStatsAsync()
        {
            var currentPlayerGrain = GrainFactory.GetGrain<IPlayerGrain>(_game.State.ReadyPlayerIds[_game.State.CurrentPlayerTurnIndex]);
            var currentPlayer = await currentPlayerGrain.GetPlayerAsync();
            IPlayerGrain? lastPlayerGrain = _game.State.LastPlayerId != 0 ? GrainFactory.GetGrain<IPlayerGrain>(_game.State.LastPlayerId) : null;
            PlayerLite? lastPlayer = lastPlayerGrain != null ? await lastPlayerGrain.GetPlayerAsync() : null;

            return new GameStats 
            {
                Id = _game.State.Id,
                CurrentPlayerId = currentPlayer.Id,
                CurrentPlayerName = currentPlayer.FullName,
                LastPlayerId = lastPlayer?.Id ?? 0,
                LastPlayerName = lastPlayer?.FullName,
                LastActivityTime = _lastActivityTime,
                LastPlayedCard = _game.State.PlayedCards.Last(),
                MarketCount = _game.State.Cards.Count,
                GameLog = _game.State.GameLog,
                Status = _game.State.Status.ToString(),
                IsTurnReversed = _game.State.PlayerTurnReversed,
                Players = _players.ToArray(),
            };
        }

        public async Task<PlayerGameScore[]> GetGameLeaderboardAsync()
        {
            var playersGameScore = new List<PlayerGameScore>();

            if (_game.State.Status == GameStatus.Ended)
            {
                foreach(var playerId in _game.State.ReadyPlayerIds)
                {
                    var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
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
            var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
            return await player.GetGameCardsAsync(_game.State.Id);            
        }

        public async Task CreateGameAsync(long creatorId, CreateGameRequest request)
        {
            await StartGameAsync(creatorId, request.PlayerIds, request.IsPrivate, request.CardCount = 10);
        }

        private Task CheckPlayersInvitations(object obj)
        {
            GameState state = (GameState)obj;
            if (!state.ReadyPlayerIds.Any() && _gameRetry > 0)
            {
                foreach (var playerId in state.PlayerIds)
                {
                    var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
                    var invitation = player.GetGameInvitationAsync(state.Id).Result;

                    if (invitation.Response ?? false)
                        state.ReadyPlayerIds.Add(playerId);
                }
                _gameRetry--;
            }
            else
            {
                if (_gameRetry == 0 && !state.ReadyPlayerIds.Any()) 
                    state.Status = GameStatus.Aborted;
                else
                    state.Status = GameStatus.Started;

                return Task.CompletedTask;
            }

            BeginGame();
            return Task.CompletedTask;
        }

        private Task CheckReadyPlayersAndStartGame(object obj)
        {
            GameState state = (GameState)obj;

            if (!state.ReadyPlayerIds.Any())
                state.Status = GameStatus.Aborted;
            else
                state.Status = GameStatus.Started;

            BeginGame();
            return Task.CompletedTask;
        }

        public async Task<bool> StartGameAsync(long creatorId, long[] playerIds, bool isPrivate, int cardCount = 10)
        {
            //Init Game
            _game.State.PlayerStartCardCount = cardCount;
            _game.State.CreatorId = creatorId;
            _game.State.PlayerIds = new[] { creatorId }.Concat(playerIds).ToList();
            _game.State.CurrentPlayerTurnIndex = 0;


            var readyPlayers = new List<long>();
            if (isPrivate)
            {
                //Send invitation to players
                foreach (var playerId in playerIds)
                {
                    var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
                    await player.SendGameInvitationsAsync(_game.State.Id, _game.State.CreatorId);
                }

                //wait for 60 secs Then Check players response to game invitations
                _timer = RegisterTimer(CheckPlayersInvitations, _game.State, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            }
            else
            {
                //await Task.Delay(36000); //wait for 120 seconds for people to join
                _timer = RegisterTimer(CheckReadyPlayersAndStartGame, _game.State, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));
            }

            return readyPlayers.Any();
        }

        private void BeginGame()
        {
            if (_game.State.ReadyPlayerIds.Any())
            {
                _game.State.ReadyPlayerIds.Add(_game.State.CreatorId);
                _game.State.Cards = GenerateGameCards();
                ShareCards(_game.State.ReadyPlayerIds);
                _game.State.Status = GameStatus.Started;
                _game.State.PlayedCards.Add(_game.State.Cards.Pop()); //Play First Card From Market
                _game.State.GameLog.Add($"Put First Card from the Market:- {_game.State.PlayedCards.Last().Name}");
                //await PopulateReadyPlayersDetails();
            }
            else
            {
                _game.State.Status = GameStatus.Aborted;
            }
            _timer.Dispose();
        }

        public Task AddPlayerAsync(long playerId)
        {
            if (_game.State.Status == GameStatus.Created && !_game.State.ReadyPlayerIds.Contains(playerId))
            {
                _game.State.PlayerIds.Add(playerId);
                _game.State.ReadyPlayerIds.Add(playerId);
            }

            return Task.CompletedTask;
        }

        public async Task<List<Card>> TryPickCardsAsync(long playerId, int count = 1)
        {
            if (_game.State.ReadyPlayerIds[_game.State.CurrentPlayerTurnIndex] != playerId)
                return new List<Card>(); //It is not the selected players turn //TODO throw and Handle exception message

            var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
            var cards = new List<Card>();
            for(int i = 0; i < count; i++)
            {
                cards.Add(_game.State.Cards.Pop());
            }
            await player.AddCardsAsync(_game.State.Id, cards);
            _game.State.GameLog.Add($"Player:{playerId} - {(await player.GetPlayerAsync()).FullName} picked cards: {string.Join('|', cards.Select(x => $"{x.Id} - {x.Name}"))}");
            _lastActivityTime = DateTime.Now;

            UpdatePlayerTurn();
            return cards;
        }

        public async Task<bool> TryPlayCard(long playerId, int cardId, CardColor? cardColor)
        {
            if (_game.State.ReadyPlayerIds[_game.State.CurrentPlayerTurnIndex] != playerId)
                throw new ValidationException("It is not your turn");

            var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);

            var card = await player.TryPlayCardAsync(_game.State.Id, cardId, _game.State.PlayedCards.Last());

            if (card == null)
                return false;

            if (cardColor != null)
                card.Color = cardColor;

            _game.State.PlayedCards.Add(card);

            _game.State.GameLog.Add($"Player:{playerId} - {(await player.GetPlayerAsync()).FullName} played card:{card.Id} - {card.Name}");
            _lastActivityTime = DateTime.Now;

            var cards = await player.GetGameCardsAsync(_game.State.Id);

            //Check if player has played all their cards
            if (!cards.Any())
            {
                EndGame(playerId);
                return true;
            }
            _game.State.LastPlayerId= playerId;
            UpdatePlayerTurn();
            return true;
        }

        private void EndGame(long playerId)
        {
            _game.State.GameLog.Add($"Game Won by Player {playerId}");
            _game.State.Status = GameStatus.Ended;
            //Propergate message to FE that the game has ended
            //TODO do cleanup after game ends
        }

        private void UpdatePlayerTurn()
        {
            var turnIncreament = _game.State.PlayerTurnReversed ? -1 : 1;

            _game.State.CurrentPlayerTurnIndex = (_game.State.CurrentPlayerTurnIndex + turnIncreament) % _game.State.ReadyPlayerIds.Count;
        }
        private async Task PopulateReadyPlayersDetails()
        {
            foreach (var playerId in _game.State.ReadyPlayerIds)
            {
                var pg = GrainFactory.GetGrain<IPlayerGrain>(playerId);
                _players.Add(await pg.GetPlayerAsync());
            }
        }

        private void ShareCards(List<long> playerIds)
        {
            var playerCards = new Dictionary<long, List<Card>>();

            for(int i = 0; i < _game.State.PlayerStartCardCount; i++)
            {
                foreach (var playerId in playerIds)
                {
                    //playerCards[playerId].Add(_game.State.Cards.Pop());
                    if (playerCards.ContainsKey(playerId))
                    {
                        playerCards[playerId].Add(_game.State.Cards.Pop());
                    }
                    else
                    {
                        playerCards.Add(playerId, new List<Card>() { _game.State.Cards.Pop() });
                    }
                }
            }

            foreach (var playerId in playerIds)
            {
                var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
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
                            IsSpecial = true,
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

//TODO Either User ReadyPlayerIds or PlayerIds, not both