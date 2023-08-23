using Orleans;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Enums;

namespace WhotGame.Abstractions.GrainTypes
{
    public interface IPlayerGrain : IGrainWithIntegerKey
    {
        Task<PlayerLite> GetPlayerAsync();
        Task<GameLite[]> GetGamesAsync();
        Task<Card[]> GetGameCardsAsync(long gameId);
        public Task SetGameCardsAsync(long gameId, List<Card> cards);
        Task AddCardsAsync(long gameId, List<Card> cards);
        Task<Card> TryPlayCardAsync(long gameId, int cardId, CardColor? color, CardShape? shape, Card cardToMatch, bool hasPendingPick2, bool hasPendingPick4);
        Task<GameInvitation[]> GetGameInvitationsAsync();
        Task<GameInvitation> GetGameInvitationAsync(long gameId);
        Task SendGameInvitationsAsync(long gameId, long playerId);
        Task RespondToGameInvitationsAsync(long gameId, bool response);
        Task AddPlayerToGame(long gameId);
        Task UpdateGameStatus(long gameId, GameStatus status);
    }
}
