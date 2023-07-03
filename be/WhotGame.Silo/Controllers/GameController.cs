using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Orleans;
using WhoteGame.Services;
using WhotGame.Abstractions.GrainTypes;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Data.Models;
using WhotGame.Core.Data.Models.Requests;
using WhotGame.Core.Data.Repositories;
using WhotGame.Core.Models.Requests;
using WhotGame.Silo.ViewModels;

namespace WhotGame.Silo.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GameController : BaseController
    {
        private readonly IGrainFactory _grainFactory;
        private readonly IRepository<Game> _gameRepo;
        private readonly IHubContext<GameHub> _gameHub;

        public GameController(IGrainFactory grainFactory,
            IHttpContextAccessor httpContext,
            IRepository<Game> gameRepo,
            IHubContext<GameHub> gameHub)
            :base(httpContext)
        { 
            _grainFactory = grainFactory;
            _gameRepo = gameRepo;
            _gameHub = gameHub;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        public async Task<IActionResult> GetConnectionId()
        {
            var user = GetCurrentUser();

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: $"connect_player_{user.UserId}");
        }

        [HttpPost("{group}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        public async Task<IActionResult> SendGroup(long group, PlayerLite player)
        {
            await _gameHub.Clients.Group($"connect_game_{group}").SendAsync("Game", player);
            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: "");
        }

        [HttpPost("{playerId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        public async Task<IActionResult> SendPlayer(long playerId, PlayerLite player)
        {
            await _gameHub.Clients.Group($"connect_player_{playerId}").SendAsync("player", player);
            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: "");
        }

        [HttpPost("{playerId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        public async Task<IActionResult> Send(long playerId, PlayerLite player)
        {
            await _gameHub.Clients.User($"{playerId}").SendAsync("Testp", player);
            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: "");
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<GameResponse>>), 200)]
        public async Task<IActionResult> GetGames()
        {
            //var user = GetCurrentUser();
            //var player = _grainFactory.GetGrain<IPlayerGrain>(user.UserId);
            //var games = await player.GetGamesAsync();
            var games = _gameRepo.Get().ToList().Select(x => (GameResponse)x);

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: games);
        }

        [HttpGet("{gameId}")]
        [ProducesResponseType(typeof(ApiResponse<GameStats>), 200)]
        public async Task<IActionResult> GetGameStats(long gameId)
        {
            var user = GetCurrentUser();
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var game = await gameGrain.GetGameStatsAsync();

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: game);
        }

        [HttpGet("{gameId}")]
        [ProducesResponseType(typeof(ApiResponse<GameStats>), 200)]
        public async Task<IActionResult> GetGameLeaderboard(long gameId)
        {
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var gameLeaderboard = await gameGrain.GetGameLeaderboardAsync();

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: gameLeaderboard);
        }

        [HttpGet("{gameId}")]
        [ProducesResponseType(typeof(ApiResponse<Card[]>), 200)]
        public async Task<IActionResult> GetGameCard(long gameId)
        {
            var user = GetCurrentUser();
            var playerGrain = _grainFactory.GetGrain<IPlayerGrain>(user.UserId);
            var cards = await playerGrain.GetGameCardsAsync(gameId);

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: cards, totalCount: cards.Length);
        }

        [HttpGet("{gameId}")]
        [ProducesResponseType(typeof(ApiResponse<Card[]>), 200)]
        public async Task<IActionResult> GetGameCard2(long gameId)
        {
            var user = GetCurrentUser();

            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var cards = await gameGrain.GetPlayerGameCardsAsync(playerId: user.UserId);

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: cards, totalCount: cards.Length);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        public async Task<IActionResult> CreateGame(CreateGameRequest request)
        {
            var user = GetCurrentUser();

            var game = new Game
            {
                CreatorId = user.UserId,
                DateCreated = DateTime.Now,
                Status = Core.Enums.GameStatus.Created,
                IsPrivate = request.IsPrivate
            };
            _gameRepo.Insert(game);
            await _gameRepo.SaveChangesAsync();

            game.Name = GenerateGameName(game.Id);
            await _gameRepo.SaveChangesAsync();

            var gameGrain = _grainFactory.GetGrain<IGameGrain>(game.Id);
            await gameGrain.CreateGameAsync(user.UserId, request);
            await GameHub.BroadcastNewGame(_gameHub, game);
            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: "");
        }

        private string GenerateGameName(long id)
        {
            var alphabets = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            id = 100 + id;
            var result = "";

            foreach (var c in id.ToString())
            {
                char x = (char)(c + 65);
                result += alphabets[c % 52];
            }
            return result;
        }

        [HttpPost("{gameId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        public async Task<IActionResult> JoinGame(long gameId)
        {
            if (_gameRepo.GetByID(gameId) == null)
                return NotFound("Game not Found");

            var user = GetCurrentUser();
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            await gameGrain.AddPlayerAsync(user.UserId);
            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: "");
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PlayCardRequest>), 200)]
        public async Task<IActionResult> PlayCard(PlayCardRequest model)
        {
            var user = GetCurrentUser();
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(model.GameId);
            var result = await gameGrain.TryPlayCard(user.UserId, model.CardId, model.CardColor, model.CardShape);

            return result ? ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: "") : ApiResponse(message: "failed", codes: ApiResponseCodes.ERROR, data: "", errors: new string [] {"Failed to play card"} );
        }

        [HttpPost("{gameId}")]
        [ProducesResponseType(typeof(ApiResponse<List<Card>>), 200)]
        public async Task<IActionResult> PickCards(long gameId)
        {
            var user = GetCurrentUser();
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var result = await gameGrain.TryPickCardsAsync(user.UserId);

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: result);
        }

        //[HttpGet("{gameId}")]
        //[ProducesResponseType(typeof(ApiResponse<ApprovalRequestResponse>), 200)]
        //public async Task<IActionResult> GetGameLeaderboard(long gameId)
        //{

        //}

        //[HttpPost]
        //[ProducesResponseType(typeof(ApiResponse<ApprovalRequestResponse>), 200)]
        //public async Task<IActionResult> JoinGame()
        //{

        //}
    }
}

//TODO get endpoint to show game Last Card Played, Last Player, Current Player Turn, How Many Market Remaining Last Activity
/*
#  add Validation for player turn
# Add logic for special Cards
# Add ReadyPlayers to GameStats
# Add IsReverse to GameStats
# Add Status to GameStats
# Fix lastPlayerId and Name in GameStats
# Fix Pick card Bug, the card is not added to players cards`
# Fix Update GameActivity TIme
# Feat Add End Of Game To GameLOg
# Add LeaderBoard endpoint

# Log Activity Time When Game is Started
# Handle ValidationException and return correct message
* refactor, Log for play for Gen market b4 picking
* Check and Test Logic for pick2-4 and rebound
# FIX CurrentPlayerTurnIndex, it should not have a negative value
 The fix is, when the index negative it should be added to the Length 
that is , 3 + (-1) = 2, and When it is positive we leave as is, that is

# in Generate Card Make card Id start from 1
# add and then find modulus
# Check player turn if it flows correctly
# check pick 4 increament
- Joker should not have color or shape// This might not be an issue
# Joker should need both color and shape
- Do not allow any action after game has ended //Might not be an issue as it is the winners turn but he has no cards
# Update Leaderboard endpoint to display winner and order the players according to their total value
# Add Number of cards to Leaderboard*/
