namespace Auth.API.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}
