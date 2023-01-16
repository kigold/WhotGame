using WhotGame.Core.Enums;

namespace WhotGame.Abstractions.Models
{
    public class GameHistoryItem
    {
        public long Id { get; set; }
        public long GameId { get; set; }
        public string Username { get; set; }
        public int? CardId { get; set; }
        public DateTime Created { get; set; }
        public string Decription { get; set; }
        public PlayerAction? PlayerAction { get; set; }
    }
}
