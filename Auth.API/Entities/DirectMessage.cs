using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.API.Entities
{
    public class DirectMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ConversationId))]
        public Conversation Conversation { get; set; } = null!;
    }
}
