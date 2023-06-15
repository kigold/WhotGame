namespace WhotGame.Abstractions.Models
{
    public class PlayerGameScore
    {
        public PlayerLite Player { get; set; }
        public int TotalValue { get; set; }
        public bool Winner { get; set; }
    }
}
