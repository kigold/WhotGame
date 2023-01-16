using Microsoft.AspNetCore.Mvc;
using Orleans;
using WhotGame.Abstractions.GrainTypes;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Data.Models;
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
        [ProducesResponseType(typeof(ApiResponse<List<GameLite>>), 200)]
        public async Task<IActionResult> GetGames()
        {
            //var user = GetCurrentUser();
            //var player = _grainFactory.GetGrain<IPlayerGrain>(user.UserId);
            //var games = await player.GetGamesAsync();
            var games = _gameRepo.Get().ToList();

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: games);
        }

        [HttpGet("{gameId}")]
        [ProducesResponseType(typeof(ApiResponse<List<GameLite>>), 200)]
        public async Task<IActionResult> GetGame(long gameId)
        {
            var user = GetCurrentUser();
            var playerGrain = _grainFactory.GetGrain<IPlayerGrain>(user.UserId);
            var games = await playerGrain.GetGamesAsync();

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: games);
        }

        [HttpGet("{gameId}")]
        [ProducesResponseType(typeof(ApiResponse<Card[]>), 200)]
        public async Task<IActionResult> GetGameCard(long gameId)
        {
            var user = GetCurrentUser();
            var playerGrain = _grainFactory.GetGrain<IPlayerGrain>(user.UserId);
            var games = await playerGrain.GetGameCardsAsync(gameId);

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: games);
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

        //[HttpPost]
        //[ProducesResponseType(typeof(ApiResponse<ApprovalRequestResponse>), 200)]
        //public async Task<IActionResult> PlayCard()
        //{

        //}

        //[HttpPost]
        //[ProducesResponseType(typeof(ApiResponse<ApprovalRequestResponse>), 200)]
        //public async Task<IActionResult> PickCards()
        //{

        //}
    }
}
