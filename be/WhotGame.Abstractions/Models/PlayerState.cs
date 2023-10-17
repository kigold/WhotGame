using WhotGame.Core.Data.Models;
using WhotGame.Core.Enums;

namespace WhotGame.Abstractions.Models
{
    public class PlayerState
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public PlayerStatus Status { get; set; }
        public Dictionary<long, List<Card>> GameCards = new();
        public Dictionary<long, GameInvitation> Invitations = new();
        public Dictionary<long, GameLite> Games = new();
    }

    public class PlayerLite
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public static implicit operator PlayerLite(PlayerState model)
        {
            return model == null ? null : new PlayerLite
            {
                Id = model.Id,
                Name = model.Name,
                Avatar = model.Avatar
            };
        }
    }
}
