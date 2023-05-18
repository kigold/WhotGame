namespace WhotGame.Abstractions.Models
{
    public class GameInvitation
    {
        public long PlayerId { get; set; }
        public long GameId { get; set; }
        public bool? Response { get; set; }
    }
}
    