﻿using WhotGame.Core.Data.Models;

namespace WhotGame.Core.DTO.Response
{
    public class GameResponse
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatorId { get; set; }
        public bool IsPrivate { get; set; }

        public static implicit operator GameResponse(Game model)
        {
            return model == null ? null : new GameResponse
            {
                Name = model.Name,
                CreatorId = model.CreatorId,
                DateCreated = model.DateCreated,
                Id = model.Id,
                IsPrivate = model.IsPrivate,
                Status = model.Status.ToString()
            };
        }

        public static implicit operator GameResponse(PlayerActiveGame model)
        {
            return (model == null || model.Game == null) ? null : new GameResponse
            {
                Name = model.Game.Name,
                CreatorId = model.Game.CreatorId,
                DateCreated = model.Game.DateCreated,
                Id = model.Game.Id,
                IsPrivate = model.Game.IsPrivate,
                Status = model.Game.Status.ToString()
            };
        }
    }
}
