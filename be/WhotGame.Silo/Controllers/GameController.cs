using Microsoft.AspNetCore.Mvc;
using Orleans;
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

        public GameController(IGrainFactory grainFactory, IHttpContextAccessor httpContext, IRepository<Game> gameRepo)
            :base(httpContext)
        { 
            _grainFactory = grainFactory;
            _gameRepo = gameRepo;
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
        [ProducesResponseType(typeof(ApiResponse<Card[]>), 200)]
        public async Task<IActionResult> GetGameCard(long gameId)
        {
            var user = GetCurrentUser();
            var playerGrain = _grainFactory.GetGrain<IPlayerGrain>(user.UserId);
            var cards = await playerGrain.GetGameCardsAsync(gameId);

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: cards);
        }

        [HttpGet("{gameId}")]
        [ProducesResponseType(typeof(ApiResponse<Card[]>), 200)]
        public async Task<IActionResult> GetGameCard2(long gameId)
        {
            var user = GetCurrentUser();

            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var cards = await gameGrain.GetPlayerGameCardsAsync(playerId: user.UserId);

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: cards);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        public async Task<IActionResult> CreateGame(CreateGameRequest request)
        {
            var user = GetCurrentUser();
            var playerGrain = _grainFactory.GetGrain<IPlayerGrain>(user.UserId);

            var game = new Game
            {
                CreatorId = user.UserId,
                DateCreated = DateTime.Now,
                Status = Core.Enums.GameStatus.Created,
                IsPrivate = request.IsPrivate
            };
            _gameRepo.Insert(game);
            await _gameRepo.SaveChangesAsync();

            var gameGrain = _grainFactory.GetGrain<IGameGrain>(game.Id);
            await gameGrain.CreateGameAsync(user.UserId, request);
            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: "");
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
            var result = await gameGrain.TryPlayCard(user.UserId, model.CardId, model.CardColor);

            return result ? ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: "") : ApiResponse(message: "failed", codes: ApiResponseCodes.ERROR, data: "", errors: new string [] {"Failed to play card"} );
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<List<Card>>), 200)]
        public async Task<IActionResult> PickCards(PickCardRequest model)
        {
            var user = GetCurrentUser();
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(model.GameId);
            var result = await gameGrain.TryPickCardsAsync(user.UserId, model.CardsCount);

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
*Add logic for special Cards
# Add ReadyPlayers to GameStats
# Add IsReverse to GameStats
# Add Status to GameStats
# Fix lastPlayerId and Name in GameStats
# Fix Pick card Bug, the card is not added to players cards`
# Fix Update GameActivity TIme
# Feat Add End Of Game To GameLOg
# Add LeaderBoard endpoint*/
