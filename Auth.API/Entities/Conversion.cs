using Auth.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace Auth.API.Entities
{
    public class Conversation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public ConversationType Type { get; set; } = ConversationType.Direct;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<DirectMessage> Messages { get; set; } = new List<DirectMessage>();
    }
}
