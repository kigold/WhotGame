namespace WhotGame.Core.Models.Requests
{
    public class CreateGameRequest
    {
        public bool IsPrivate { get; set; }
        public long[] PlayerIds { get; set; } = new long[] { }; 
        public int CardCount { get; set; }
        public int PlayersCount { get; set; }
    }
}
