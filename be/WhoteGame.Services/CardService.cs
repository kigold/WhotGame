using WhotGame.Abstractions.Models;
using WhotGame.Core.Data.Models;
using WhotGame.Core.Enums;
using static WhotGame.Abstractions.Constants;

namespace WhoteGame.Services
{
    public interface ICardService
    {
        Task<List<Card>> GenerateCards();
    }
    public class CardService : ICardService
    {
        public Task<List<Card>> GenerateCards()
        {
            return Task.FromResult(GenerateGameCards());
        }

        private static List<Card> GenerateGameCards()
        {
            var cards = new List<Card>();
            int count = 1;

            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                foreach (CardShape shape in Enum.GetValues(typeof(CardShape)))
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        cards.Add(new Card
                        {
                            Id = count++,
                            Color = color,
                            Shape = shape,
                            IsSpecial = false,
                            Name = i.ToString(),
                            Value = i
                        });
                    }
                    foreach (var special in SpecialCards)
                    {
                        cards.Add(new Card
                        {
                            Id = count++,
                            Color = color,
                            Shape = shape,
                            IsSpecial = true,
                            Name = special.Item1,
                            Value = special.Item2
                        });
                    }
                }
            }

            Random random = new Random();

            return cards.OrderBy(x => random.Next()).ToList();
        }

        private static (string, int)[] SpecialCards => new[] { (JOKER, 50), (PICK2, 50), (PICK4, 50), (GENERAL_MARKET, 50), (HOLD_ON, 50), (REVERSE, 50) };
    }
}
