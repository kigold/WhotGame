namespace WhotGame.Silo.ViewModels
{
    public class CreateUserRequest
    {
        public string Email { get ; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get ; set; }
    }

    public record UserResponse(long id, string email, string firstname, string lastname);
}
