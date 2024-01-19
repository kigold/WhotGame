using WhotGame.Core.Data.Models;

namespace WhotGame.Core.DTO.Response
{
    public class CardResponse
    {
        public int Id { get; set; }
        public string? Color { get; set; }
        public string? Shape { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public bool IsSpecial { get; set; }

        public static implicit operator CardResponse(Card model)
        {
            return model == null ? new CardResponse() : new CardResponse
            {
                Name = model.Name,
                Id = model.Id,
                Color = model.Color.ToString(),
                Shape = model.Shape.ToString(),
                Value = model.Value,
                IsSpecial = model.IsSpecial
            };
        }
    }
}
