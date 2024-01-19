using WhotGame.Core.Enums;

namespace WhotGame.Core.Data.Models
{
    public class Card
    {
        public int Id { get; set; }
        public CardColor? Color { get; set; }
        public CardShape? Shape { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public bool IsSpecial { get; set; }
    }
}
