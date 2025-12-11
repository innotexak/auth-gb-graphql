namespace Auth.API.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string RefreshToken {  get; set; } = string.Empty;

        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
