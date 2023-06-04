using WhotGame.Core.Data.Models;
using WhotGame.Core.Enums;

namespace WhotGame.Silo.ViewModels
{
    public class GameResponse
    {
        public long Id { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatorId { get; set; }
        public bool IsPrivate { get; set; }

        public static implicit operator GameResponse(Game model)
        {
            return model == null ? new GameResponse() : new GameResponse
            {
                CreatorId = model.CreatorId,
                DateCreated = model.DateCreated,
                Id = model.Id,
                IsPrivate = model.IsPrivate,
                Status = model.Status.ToString()
            };
        }
    }
}
