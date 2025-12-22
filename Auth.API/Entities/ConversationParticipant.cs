using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.API.Entities
{
    public class ConversationParticipant
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ConversationId))]
        public Conversation Conversation { get; set; } = null!;
    }
}
