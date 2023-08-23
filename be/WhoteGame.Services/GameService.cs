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
        Task<GameResponse?> GetActiveGame();
        Task<GameResponse> CreateGame(long userId);
        Task UpdateGameStatus(long gameId, UpdateGameStatus request);
    }
    public class GameService : IGameService
    {
        private readonly IRepository<Game> _gameRepo;
        public GameService(IRepository<Game> gameRepo) 
        { 
            _gameRepo = gameRepo;
        }

        public async Task<GameResponse[]> GetGames()
        {
            return _gameRepo.Get().ToList()
                    .Where(x => x.Status == GameStatus.Created)
                .OrderByDescending(x => x.DateCreated)
                .Select(x => (GameResponse)x).ToArray();
        }

        public async Task<GameResponse?> GetActiveGame()
        {
            return _gameRepo.Get().ToList()
            .Where(x => x.Status == GameStatus.Created || x.Status == GameStatus.Started)
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
                DateCreated = DateTime.Now,
                Status = GameStatus.Created,
                IsPrivate = false
            };
            _gameRepo.Insert(game);
            await _gameRepo.SaveChangesAsync();

            game.Name = GenerateGameName(game.Id);
            await _gameRepo.SaveChangesAsync();
            return game;
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
