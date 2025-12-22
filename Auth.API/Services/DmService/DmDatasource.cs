using Auth.API.Data;
using Auth.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Services.DmService
{
    public class DmDatasource
    {
        private readonly AuthDBContext _db;

        public DmDatasource(AuthDBContext db)
        {
            _db = db;
        }

        // Find or create DM conversation between two users
        public async Task<Conversation> GetOrCreateConversationAsync(
            Guid userA,
            Guid userB
        )
        {

            var conversation = await _db.Conversations
                .Include(c => c.Participants)
                .Where(c =>
                    c.Participants.Count == 2 &&
                    c.Participants.Any(p => p.UserId == userA) &&
                    c.Participants.Any(p => p.UserId == userB)
                )
                .FirstOrDefaultAsync();

            if (conversation != null)
                return conversation;

            // Create new conversation
            conversation = new Conversation
            {
                Id = Guid.NewGuid()
            };

            _db.Conversations.Add(conversation);
            await _db.SaveChangesAsync();

            _db.ConversationParticipants.AddRange(
                new ConversationParticipant
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    UserId = userA
                },
                new ConversationParticipant
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    UserId = userB
                }
            );

            await _db.SaveChangesAsync();

            return conversation;
        }

        public async Task<DirectMessage> SaveMessageAsync(
            Guid conversationId,
            Guid senderId,
            string content
        )
        {
            var message = new DirectMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content
            };

            _db.DirectMessages.Add(message);
            await _db.SaveChangesAsync();

            return message;
        }

        // Load message history for a conversation (ascending by CreatedAt)
        public async Task<List<DirectMessage>> GetMessagesAsync(Guid conversationId, int limit = 200)
        {
            return await _db.DirectMessages
                .Where(dm => dm.ConversationId == conversationId)
                .OrderBy(dm => dm.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        // Get all users on the platform (optionally exclude current user)
    }
}