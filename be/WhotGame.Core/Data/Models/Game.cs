using WhotGame.Core.Enums;

namespace WhotGame.Core.Data.Models
{
    public class Game
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public long CreatorId { get; set; }
        public GameStatus Status { get; set; }
        public bool IsPrivate { get; set; }
    }
}