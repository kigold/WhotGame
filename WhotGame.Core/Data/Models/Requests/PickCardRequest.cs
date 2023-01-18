namespace WhotGame.Core.Data.Models.Requests
{
    public class PickCardRequest
    {
        public long GameId { get; set; }
        public int CardsCount { get; set; }
    }
}
