namespace WhotGame.Core.DTO.Response
{
    public class GameLog
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Color { get; set; }
    }
}
