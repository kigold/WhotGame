﻿using WhotGame.Core.Enums;

namespace WhotGame.Abstractions.Models
{
    public class GameState
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public GameStatus Status { get; set; }
        public List<long> PlayerIds { get; set; } = new List<long>();
        public long CreatorId { get; set; }
        public long WinnerId { get; set; }
        public List<Card> Cards { get; set; } = new List<Card>();
        public int CardCount { get; set; }
        public int CurrentPlayerTurnIndex { get; set; }
        public bool PlayerTurnReversed { get; set; }
        public Card LastPlayedCard { get; set; }
        public List<string> GameLog { get; set; } = new List<string>();
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
}