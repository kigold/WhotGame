namespace WhotGame.Core.DTO.Response
{
    public class GameLog
    {
        public GameLog() { }
        public GameLog(int id, string message, string? color) 
        {
            Id = id;
            Message = message;
            Color = color;
        }
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Color { get; set; }
    }

    public class GameLogResponse
    {
        public GameLog[] Logs { get; set; } = new GameLog[0];
        public bool HasMore { get; set; }
        public int Skip { get; set; }
    }
}
