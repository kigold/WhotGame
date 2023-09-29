namespace WhotGame.Core.Data.Models
{
    public class PlayerActiveGame
    {
        public long GameId { get; set; }
        public long PlayerId { get; set; }
        public DateTime DateCreated { get; set; }
        public Game Game { get; set; }
    }
}
