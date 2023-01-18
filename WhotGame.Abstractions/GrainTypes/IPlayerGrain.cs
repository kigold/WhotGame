using Orleans;
using WhotGame.Abstractions.Models;

namespace WhotGame.Abstractions.GrainTypes
{
    public interface IPlayerGrain : IGrainWithIntegerKey
    {
        Task<PlayerLite> GetPlayerAsync();
        Task<GameLite[]> GetGamesAsync();
        Task<Card[]> GetGameCardsAsync(long gameId);
        public Task SetGameCardsAsync(long gameId, List<Card> cards);
        Task AddCardsAsync(long gameId, List<Card> cards);
        Task<Card> TryPlayCardAsync(long gameId, int cardId, Card cardToMatch);
        Task<GameInvitation[]> GetGameInvitationsAsync();
        Task<GameInvitation> GetGameInvitationAsync(long gameId);
        Task SendGameInvitationsAsync(long gameId, long playerId);
        Task RespondToGameInvitationsAsync(long gameId, bool response);
    }
}
