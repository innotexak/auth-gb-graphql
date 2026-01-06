using Auth.API.Entities;
using Auth.API.Enums;
using System.Reflection;

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


        public class GroupInputDto
        {
            public Guid? Id { get; set; }
            public required string Title { get; set; }
            public required string Description { get; set; }

            public GroupStatus Status { get; set; } = GroupStatus.Active;

        }


        public class GroupRespondeDto
        {
            public Guid Id { get; set; }
            public  string Title { get; set; }
            public  string Description { get; set; }

            public GroupStatus Status { get; set; } 

            public  Guid UserId { get; set; }

            public User? User { get; set; }

        }

     

    }
}
