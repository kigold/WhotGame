using Microsoft.AspNetCore.SignalR;
using Orleans;
using Orleans.Runtime;
using System.ComponentModel.DataAnnotations;
using WhoteGame.Services;
using WhotGame.Abstractions.Extensions;
using WhotGame.Abstractions.GrainTypes;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Data.Models;
using WhotGame.Core.Data.Models.Requests;
using WhotGame.Core.DTO.Response;
using WhotGame.Core.Enums;
using WhotGame.Core.Models.Requests;
using static WhotGame.Abstractions.Constants;

namespace WhotGame.Grains
{
    public class GameGrain: Grain, IGameGrain
    {
        private readonly IHubContext<GameHub> _gameHub;
        private readonly IPersistentState<GameState> _game;
        private readonly IGameService _gameService;
        private readonly ICardService _cardService;
        private List<PlayerLite> _players = new List<PlayerLite>();
        private int _checkInvitationRetry = 3;
        private IDisposable? _startGameTimer;
        private DateTime _lastActivityTime;
        private int _pick2Multiplier = 0;
        private int _pick4Multiplier = 0;
        private SkipPlayerType _skipPlayer = SkipPlayerType.None;
        private int MAX_PLAYERS = 10;

        public GameGrain([PersistentState("Game", "WhotGame")] IPersistentState<GameState> game,
            IGameService gameService,
            ICardService cardService,
            IHubContext<GameHub> gameHub) 
        { 
            _game = game;
            _gameHub = gameHub;
            _gameService = gameService;
            _cardService = cardService;
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
                CurrentPlayerName = currentPlayer.Name,
                LastPlayerId = lastPlayer?.Id ?? 0,
                LastPlayerName = lastPlayer?.Name ?? string.Empty,
                LastActivityTime = _lastActivityTime,
                LastPlayedCard = _game.State.PlayedCards.LastOrDefault(),
                MarketCount = _game.State.Cards.Count,
                GameLog = _game.State.GameLog,
                Status = _game.State.Status.ToString(),
                IsTurnReversed = _game.State.PlayerTurnReversed,
                Players = _players.ToArray(),
                Pick2Count = _pick2Multiplier * 2,
                Pick4Count= _pick4Multiplier * 4,
            };
        }

        public async Task<PlayerGameScore[]> GetGameLeaderboardAsync()
        {
            var playersGameScore = new List<PlayerGameScore>();

            if (_game.State.Status == GameStatus.Ended)
            {
                playersGameScore.Add(new PlayerGameScore { Winner = true,TotalValue = 0, Player = _players.First(x => x.Id == _game.State.WinnerId) });
                foreach (var playerId in _game.State.ReadyPlayerIds)
                {
                    if (playerId == _game.State.WinnerId)
                        continue;

                    var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
                    var cards = await player.GetGameCardsAsync(_game.State.Id);
                    playersGameScore.Add(new PlayerGameScore
                    {
                        Player = _players.First(x => x.Id == playerId),
                        TotalValue = cards.Sum(x => x.Value)
                    });
                }
            }

            throw new ValidationException("Game has not ended or cannot be found");
        }

        public async Task<Card[]> GetPlayerGameCardsAsync(long playerId)
        {
            var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
            return await player.GetGameCardsAsync(_game.State.Id);            
        }

        public async Task StartGameAsync(long creatorId, CreateGameRequest request)
        {
            await StartGameAsync(creatorId, request.PlayerIds, request.IsPrivate, request.CardCount = 30);
        }

        private async Task CheckPlayersInvitations(object obj)
        {
            GameState state = (GameState)obj;
            if (!state.ReadyPlayerIds.Any() && _checkInvitationRetry > 0) //TODO Check how this works
            {
                foreach (var playerId in state.PlayerIds)
                {
                    var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
                    var invitation = player.GetGameInvitationAsync(state.Id).Result;

                    if (invitation.Response ?? false)
                        state.ReadyPlayerIds.Add(playerId);
                }
                _checkInvitationRetry--;
            }
            else
            {
                if (_checkInvitationRetry == 0 && !state.ReadyPlayerIds.Any()) 
                    state.Status = GameStatus.Aborted;
                else
                    state.Status = GameStatus.Started;
            }

            await BeginGame();
        }

        private async Task CheckReadyPlayersAndStartGame(object obj)
        {
            GameState state = (GameState)obj;

            if (state.ReadyPlayerIds.Count < 2)
                state.Status = GameStatus.Aborted;
            else
                state.Status = GameStatus.Started;

            await BeginGame();
        }

        public async Task<bool> StartGameAsync(long creatorId, long[] playerIds, bool isPrivate, int cardCount = 10)
        {
            //Init Game
            _game.State.PlayerStartCardCount = cardCount;
            _game.State.CreatorId = creatorId;
            //_game.State.PlayerIds = new[] { creatorId }.Concat(playerIds).ToList();
            //_game.State.ReadyPlayerIds = new[] { creatorId }.Concat(playerIds).ToList();
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
                _startGameTimer = RegisterTimer(CheckPlayersInvitations, _game.State, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            }
            else
            {
                //await Task.Delay(36000); //wait for 120 seconds for people to join
                _startGameTimer = RegisterTimer(CheckReadyPlayersAndStartGame, _game.State, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(10));
            }

            return readyPlayers.Any();
        }

        private async Task BeginGame()
        {
            if (_game.State.Status == GameStatus.Started)
            {
                //_game.State.ReadyPlayerIds.Add(_game.State.CreatorId);
                _game.State.Cards = await _cardService.GenerateCards();
                ShareCards(_game.State.ReadyPlayerIds);
                _game.State.Status = GameStatus.Started;
                _game.State.PlayedCards.Add(_game.State.Cards.Pop()); //Play First Card From Market
                _game.State.GameLog.Add($"Put First Card from the Market:- {_game.State.PlayedCards.Last().Name}");
                await PopulateReadyPlayersDetails();
                _lastActivityTime = DateTime.UtcNow;
                await GameHub.BroadcastStartGame(_gameHub, _game.State.Id);
                await _gameService.UpdateGameStatus(_game.State.Id, new UpdateGameStatus { Status = _game.State.Status });
            }
            else
            {
                //_game.State.Status = GameStatus.Aborted;
                await GameHub.BroadcastAbortGame(_gameHub, _game.State.Id);
                _game.State.PlayerIds.ForEach(async x => await  _gameService.RemovePlayerFromGame(x, _game.State.Id));
            }
            await _gameService.UpdateGameStatus(_game.State.Id, new UpdateGameStatus { Status = _game.State.Status });
            _startGameTimer?.Dispose();
        }

        public async Task<bool> AddPlayerAsync(long playerId)
        {
            if (_game.State.Status != GameStatus.Created || _game.State.ReadyPlayerIds.Count() >= MAX_PLAYERS)
            {
                return false;
            }
            if (_game.State.Status == GameStatus.Created && !_game.State.ReadyPlayerIds.Contains(playerId) && _game.State.ReadyPlayerIds.Count() < MAX_PLAYERS)
            {
                //TODO do I really need two list of players? probably for Private game
                _game.State.PlayerIds.Add(playerId);
                _game.State.ReadyPlayerIds.Add(playerId);
                var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
                await player.AddPlayerToGame(_game.State.Id);
                return true;
            }

            return true;
        }

        public async Task<List<Card>> TryPickCardsAsync(long playerId)
        {
            var cardsToPickCount = 1;
            if (_game.State.ReadyPlayerIds[_game.State.CurrentPlayerTurnIndex] != playerId)
                throw new ValidationException("It is not your turn");

            switch (_game.State.PlayedCards.LastOrDefault()?.Name)//Get CardCount from pick-multiplier where applicable
            {
                case PICK2:
                    {
                        cardsToPickCount = 2 * _pick2Multiplier;
                        break;
                    }
                case PICK4:
                    {
                        cardsToPickCount = 4 * _pick4Multiplier;
                        break;
                    }
            }

            var cards = await PickCard(playerId, cardsToPickCount);

            //Reset pick2 and Pick4 Counter
            _pick2Multiplier = 0;
            _pick4Multiplier = 0;
            await UpdatePlayerTurn();
            return cards;
        }

        private async Task<List<Card>> PickCard(long playerId, int count)
        {
            var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
            var cards = new List<Card>();
            for (int i = 0; i < count; i++)
            {
                cards.Add(_game.State.Cards.Pop());
            }
            await player.AddCardsAsync(_game.State.Id, cards);
            _game.State.GameLog.Add($"Player:{playerId} - {(await player.GetPlayerAsync()).Name} picked cards: {string.Join("| ", cards.Select(x => $"{x.Id} - {x.Name} - {x.Shape}"))}");
            _lastActivityTime = DateTime.UtcNow;
            return cards;
        }

        public async Task<bool> TryPlayCard(long playerId, int cardId, CardColor? cardColor, CardShape? cardShape)
        {
            if (_game.State.ReadyPlayerIds[_game.State.CurrentPlayerTurnIndex] != playerId)
                throw new ValidationException("It is not your turn");

            var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);

            var card = await player.TryPlayCardAsync(_game.State.Id, cardId, cardColor, cardShape, _game.State.PlayedCards.Last(), _pick2Multiplier > 0, _pick4Multiplier > 0);

            if (string.Equals(card.Name, JOKER))
            {
                card.Color = cardColor;
                card.Shape = cardShape;
            }
                

            await ProcessPlayedCard(card, playerId);

            _game.State.PlayedCards.Add(card);

            _game.State.GameLog.Add($"Player:{playerId} - {(await player.GetPlayerAsync()).Name} played card:{card.Id} - {card.Name} - {card.Shape}");
            _lastActivityTime = DateTime.UtcNow;

            var cards = await player.GetGameCardsAsync(_game.State.Id);

            //Check if player has played all their cards
            if (!cards.Any())
            {
                EndGame(playerId);
                return true;
            }
            _game.State.LastPlayerId= playerId;
            await UpdatePlayerTurn();
            await GameHub.BroadcastCardPlayed(_gameHub, _game.State.Id, (CardResponse)card);
            return true;
        }

        private async Task ProcessPlayedCard(Card card, long currentPlayerId)
        {
            //Handle Reverse
            //handle return of Pick2 and pick4
            switch (card.Name)
            {
                case GENERAL_MARKET:
                    {
                        //Pick Cards for All Player
                        foreach (var playerId in _game.State.ReadyPlayerIds)
                        {
                            if (playerId == currentPlayerId)
                                continue;
                            var cards = await PickCard(playerId, 1);
                            await GameHub.BroadcastReceivedCards(_gameHub, _game.State.Id, playerId, cards.Select(x => (CardResponse)x).ToArray());
                        }
                        _skipPlayer = SkipPlayerType.SkipAllPlayers;
                        break;
                    }
                case PICK2:
                    {
                        _pick2Multiplier++;
                        break;
                    }
                case PICK4:
                    {
                        _pick4Multiplier++;
                        break;
                    }
                case REVERSE:
                    {
                        _game.State.PlayerTurnReversed = !_game.State.PlayerTurnReversed;
                        break;
                    }
                case HOLD_ON:
                    {
                        _skipPlayer = SkipPlayerType.SkipSinglePlayer;
                        break;
                    }
            }
        }

        private void EndGame(long playerId)
        {
            _game.State.GameLog.Add($"Game Won by Player {playerId}");
            _game.State.Status = GameStatus.Ended;
            _game.State.WinnerId = playerId;
            //Propergate message to FE that the game has ended
            //TODO do cleanup after game ends
            _game.State.ReadyPlayerIds.ForEach(x =>
            {
                var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
                player.EndGame(_game.State.Id);
            });
        }

        private async Task UpdatePlayerTurn()
        {
            int increamentValue = 1;
            switch (_skipPlayer)
            {
                case SkipPlayerType.SkipSinglePlayer:
                    {
                        increamentValue++;
                        break;
                    }
                case SkipPlayerType.SkipAllPlayers:
                    {
                        increamentValue = _game.State.ReadyPlayerIds.Count;
                        break;
                    }
                default: break;
            }
            Console.WriteLine($"INCREMENT --------------------{increamentValue}");
            var turnIncreament = _game.State.PlayerTurnReversed ? -increamentValue : increamentValue;

            _game.State.CurrentPlayerTurnIndex = (_game.State.CurrentPlayerTurnIndex + turnIncreament);

            //To Handle Negative Indexes
            _game.State.CurrentPlayerTurnIndex = _game.State.CurrentPlayerTurnIndex > 0 
                ? _game.State.CurrentPlayerTurnIndex % _game.State.ReadyPlayerIds.Count :
                (_game.State.CurrentPlayerTurnIndex + _game.State.ReadyPlayerIds.Count) % _game.State.ReadyPlayerIds.Count;
            //TODO send player turn to all players
            await GameHub.BroadcastUpdateTurn(_gameHub, _game.State.Id, _players[_game.State.CurrentPlayerTurnIndex]);
            _skipPlayer = SkipPlayerType.None; //Reset _skipOnrPlayer
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
    }
}
//TODO Broadcast LogMessage for each required action, 
//TODO Either User ReadyPlayerIds or PlayerIds, not both
//TODO I might have to move stuff that I need to be persisted to the grain
// TODO figure out how to close Ophaned games that are still in created status, maybe background service that clean up games