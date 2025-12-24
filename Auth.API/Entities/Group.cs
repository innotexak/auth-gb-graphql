using Auth.API.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.API.Entities
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GroupStatus Status { get; set; }


        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public Guid UserId { get; set; }

        public User User { get; set; }  
    }
}
