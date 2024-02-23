namespace WhotGame.Core.Models.Requests
{
    public class CreateGameRequest
    {
        public bool IsPrivate { get; set; }
        public long[] PlayerIds { get; set; } = new long[] { };
        public int CardCount { get; set; }
    }

    public class JoinGameRequest
    {
        public string GameMode { get; set; }
    }
}
