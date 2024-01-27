using Microsoft.EntityFrameworkCore;
using WhotGame.Core.Data.Models;
using WhotGame.Core.Data.Models.Requests;
using WhotGame.Core.Data.Repositories;
using WhotGame.Core.DTO.Response;
using WhotGame.Core.Enums;

namespace WhoteGame.Services
{
    public interface IGameService
    {
        Task<GameResponse?> GetGameToJoin(long userId);
        Task<GameResponse[]> GetGames();
        Task<GameResponse?> GetAvailableGame();
        Task<GameResponse?> GetMyActiveGame(long playerId);
        Task<GameResponse> CreateGame(long userId);
        Task AddPlayerToGame(long playerId, long gameId);
        Task RemovePlayerFromGame(long playerId, long gameId);
        Task UpdateGameStatus(long gameId, UpdateGameStatus request);
    }
    public class GameService : IGameService
    {
        private readonly IRepository<Game> _gameRepo;
        private readonly IRepository<PlayerActiveGame> _playerActiveGameRepo;
        public GameService(IRepository<Game> gameRepo, IRepository<PlayerActiveGame> playerActiveGameRepo) 
        { 
            _gameRepo = gameRepo;
            _playerActiveGameRepo = playerActiveGameRepo;
        }

        public async Task<GameResponse[]> GetGames()
        {
            return _gameRepo.Get().ToList()
                    .Where(x => x.Status == GameStatus.Created)
                .OrderByDescending(x => x.DateCreated)
                .Select(x => (GameResponse)x).ToArray();
        }

        public async Task<GameResponse?> GetAvailableGame()
        {
            return _gameRepo.Get().ToList()
            .Where(x => x.Status == GameStatus.Created)
                .OrderByDescending(x => x.DateCreated)
                .Select(x => (GameResponse)x).FirstOrDefault();
        }

        public async Task<GameResponse?> GetGameToJoin(long userId)
        {
            var games = _gameRepo.Get().ToList()
                .Where(x => x.Status == GameStatus.Created
                    && x.DateCreated > DateTime.Parse("2023-08-06") //TODO Remove, this is just to filter out stale games
                )
                .OrderByDescending(x => x.DateCreated);

            if (games.Any())
                return games.First();

            return null;
        }

        public async Task<GameResponse> CreateGame(long userId)
        {
            var game = new Game
            {
                CreatorId = userId,
                DateCreated = DateTime.UtcNow,
                Status = GameStatus.Created,
                IsPrivate = false
            };
            _gameRepo.Insert(game);
            await _gameRepo.SaveChangesAsync();

            game.Name = GenerateGameName(game.Id);
            await _gameRepo.SaveChangesAsync();
            return game;
        }

        //This repo is required to keep track of players active games.
        //This might be removed if I am able to Persist Player Games using Orleans state, and if it can be retrieved even when the system restarts
        public async Task<GameResponse?> GetMyActiveGame(long playerId)
        {
            var query = _playerActiveGameRepo
                .Get(x => x.PlayerId == playerId)
                .Where(x => x.Game.Status == GameStatus.Started || x.Game.Status == GameStatus.Created)
                .OrderByDescending(x => x.DateCreated)
                .Include(x => x.Game);

            return (GameResponse)query.FirstOrDefault()!;
        }

        //Should be revised, similar to GetMyActiveGame
        public async Task AddPlayerToGame(long playerId, long gameId)
        {
            _playerActiveGameRepo.Insert(new PlayerActiveGame { GameId = gameId, PlayerId = playerId, DateCreated = DateTime.UtcNow });
            await _playerActiveGameRepo.SaveChangesAsync();
        }

        //Should be revised, similar to GetMyActiveGame
        public async Task RemovePlayerFromGame(long playerId, long gameId)
        {
            //var item = _playerActiveGameRepo.Get(x => x.PlayerId == playerId && x.GameId == gameId);
            try
            {
                _playerActiveGameRepo.Delete(playerId, gameId);
                await _playerActiveGameRepo.SaveChangesAsync();
            }
            catch { }
        }

        public async Task UpdateGameStatus(long gameId, UpdateGameStatus request)
        {
            var game = _gameRepo.GetByID(gameId);
            if (game != null)
            {
                game.Status = request.Status;
                await _gameRepo.SaveChangesAsync();
            } 
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
    }
}
