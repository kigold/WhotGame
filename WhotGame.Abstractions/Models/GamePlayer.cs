namespace WhotGame.Abstractions.Models
{
    public class GamePlayer
    {
        public long GameId { get; set; }
        public long PlayerId { get; set; }
        public Game Game { get; set; }
        public Player Player { get; set; }
    }
}
