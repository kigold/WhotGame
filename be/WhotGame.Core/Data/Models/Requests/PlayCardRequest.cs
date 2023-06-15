using System.ComponentModel.DataAnnotations;
using WhotGame.Core.Enums;

namespace WhotGame.Core.Data.Models.Requests
{
    public class PlayCardRequest
    {
        [Required]
        public long GameId { get; set; }
        [Required]
        public int CardId { get; set; }
        public CardColor? CardColor { get; set; }
        public CardShape? CardShape { get; set; }
    }
}
