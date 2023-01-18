using Orleans;
using WhotGame.Core.Enums;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Models.Requests;

namespace WhotGame.Abstractions.GrainTypes
{
    public interface IGameGrain : IGrainWithIntegerKey
    {
        Task<GameLite> GetGamesAsync();
        Task<GameStats> GetGameStatsAsync();
        Task CreateGameAsync(long creatorId, CreateGameRequest request);
        Task<bool> StartGameAsync(long creatorId, long[] playerIds, bool isPrivate, int cardCount); //Create Game and Send Invitation to players and then start the game
        Task<PlayerGameScore[]> GetGameLeaderboardAsync(); //should only return when the game has ended
        Task AddPlayerAsync(long playerId);
        Task<List<Card>> TryPickCardsAsync(long playerId, int count);

        Task<Card[]> GetPlayerGameCardsAsync(long playerId);
        //Task<Card> TryPickCardAsync();
        Task<bool> TryPlayCard(long playerId, int cardId, CardColor? cardColor);
    }
}