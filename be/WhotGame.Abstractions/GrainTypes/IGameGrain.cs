using Orleans;
using WhotGame.Core.Enums;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Models.Requests;
using WhotGame.Core.Data.Models;
using WhotGame.Core.DTO.Response;

namespace WhotGame.Abstractions.GrainTypes
{
    public interface IGameGrain : IGrainWithIntegerKey
    {
        Task<GameLite> GetGamesAsync();
        Task<GameLogResponse> GetGameLogsAsync(int skip, int pageSize);
        Task<GameStats> GetGameStatsAsync();
        Task StartGameAsync(long creatorId, CreateGameRequest request);
        Task<bool> StartGameAsync(long creatorId, long[] playerIds, bool isPrivate, int cardCount); //Create Game and Send Invitation to players and then start the game
        Task<PlayerGameScore[]> GetGameLeaderboardAsync(); //should only return when the game has ended
        Task<bool> AddPlayerAsync(long playerId, GameMode gameMode = GameMode.None);
        Task<List<Card>> TryPickCardsAsync(long playerId);

        Task<Card[]> GetPlayerGameCardsAsync(long playerId);
        //Task<Card> TryPickCardAsync();
        Task<bool> TryPlayCard(long playerId, int cardId, CardColor? cardColor, CardShape? cardShape);
    }
}