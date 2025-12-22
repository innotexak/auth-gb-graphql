namespace Auth.API.Services.DmService
{
    public class DmDTOs
    {

        public class UserDto
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
        }

        public class AllUsersData
        {
            public List<UserDto> AllUsers { get; set; } = new();
        }

        public class TypingEvent
        {
            public Guid ConversationId { get; set; }
            public Guid UserId { get; set; }
            public bool IsTyping { get; set; }
        }
    }
}
