using WhotGame.Core.Data.Models;
using WhotGame.Core.DTO.Response;
using WhotGame.Core.Enums;

namespace WhotGame.Abstractions.Models
{
    public class GameState
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public GameStatus Status { get; set; }
        public List<long> PlayerIds { get; set; } = new List<long>();
        public List<long> ReadyPlayerIds { get; set; } = new List<long>();
        public long CreatorId { get; set; }
        public long WinnerId { get; set; }
        public List<Card> Cards { get; set; } = new List<Card>();
        public int PlayerStartCardCount { get; set; }
        public int CurrentPlayerTurnIndex { get; set; }
        public bool PlayerTurnReversed { get; set; }
        public List<Card> PlayedCards { get; set; } = new List<Card>();
        public long LastPlayerId { get; set; }
        public List<GameLog> GameLog { get; set; } = new List<GameLog>();
        public bool PlayerTurnWarningTriggered = false;
    }

    public class GameLite
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public GameStatus Status { get; set; }
        public long[] PlayerIds { get; set; }
        public long CreatorId { get; set; }

        public static implicit operator GameLite(GameState data)
        {
            return data == null ? null : new GameLite
            {
                Id = data.Id,
                Name = data.Name,
                Created = data.Created,
                Status = data.Status,
                CreatorId = data.CreatorId,
                PlayerIds = data.PlayerIds.ToArray(),
            };
        }
    }

    public class GameStats
    {
        public long Id { get; set; }
        public DateTime LastActivityTime { get; set; }
        public long LastPlayerId { get; set; }
        public string LastPlayerName { get; set; }
        public CardResponse? LastPlayedCard { get; set; }
        public long CurrentPlayerId { get; set; }
        public string CurrentPlayerName { get; set; }
        public int MarketCount { get; set; }
        public string Status { get; set; }
        public List<GameLog> GameLog { get; set; }
        public bool IsTurnReversed { get; set; }
        public PlayerLite[] Players { get; set; }
        public int Pick2Count { get; set; }
        public int Pick4Count { get; set; }
    }
}

