using System.ComponentModel.DataAnnotations.Schema;
using static HotChocolate.ErrorCodes;

namespace Auth.API.Entities
{
    public class Authentication
    {
        public Guid Id { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryTime { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User? User { get; set; } // nullable, no initialization
    }

}
