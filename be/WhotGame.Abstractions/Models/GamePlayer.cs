namespace WhotGame.Abstractions.Models
{
    public class GamePlayer
    {
        public long GameId { get; set; }
        public long PlayerId { get; set; }
        public GameState Game { get; set; }
        public PlayerState Player { get; set; }
    }
}
