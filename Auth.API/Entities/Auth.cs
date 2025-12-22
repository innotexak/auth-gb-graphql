using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.API.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        [InverseProperty("User")]
        public Authentication? Authentication { get; set; } 

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }

}
