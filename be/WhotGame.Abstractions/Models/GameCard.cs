using WhotGame.Core.Data.Models;

namespace WhotGame.Abstractions.Models
{
    public class GameCard
    {
        public int CardId { get; set; }
        public long GameId { get; set; }
        public Card Card { get; set; } = new();
    }
}