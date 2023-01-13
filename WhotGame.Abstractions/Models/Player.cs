using WhotGame.Abstractions.Enums;

namespace WhotGame.Abstractions.Models
{
    public class Player
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public PlayerStatus Status { get; set; }
        public Dictionary<long, List<Card>> GameCards = new();
        public Dictionary<long, GameInvitation> Invitations = new();
        public Dictionary<long, GameLite> Games = new();
    }
}
