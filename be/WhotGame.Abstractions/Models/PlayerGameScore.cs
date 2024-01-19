namespace WhotGame.Abstractions.Models
{
    public class PlayerGameScore
    {
        public PlayerLite Player { get; set; }
        public int TotalCardsValue { get; set; }
        public bool IsWinner { get; set; }
    }
}
