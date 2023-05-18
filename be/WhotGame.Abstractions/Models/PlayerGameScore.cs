namespace WhotGame.Abstractions.Models
{
    public class PlayerGameScore
    {
        public long GameId { get; set; }
        public long PlayerId { get; set; }
        public int TotalValue { get; set; }
        //public bool Winner { get; set; }
    }
}
