using Auth.API.Data;
using Auth.API.Entities;
using Auth.API.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using static Auth.API.Services.DmService.DmDTOs;

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
        public async Task<Conversation> GetOrCreateConversationAsync(Guid userA, Guid userB)
        {
            // 1. Try to find existing
            var conversation = await _db.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c =>
                    c.Participants.Count == 2 &&
                    c.Participants.Any(p => p.UserId == userA) &&
                    c.Participants.Any(p => p.UserId == userB)
                );

            if (conversation != null) return conversation;

            // 2. Create new if not found
            var newConversation = new Conversation { Id = Guid.NewGuid() };

            var participants = new List<ConversationParticipant>
                {
                    new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = newConversation.Id, UserId = userA },
                    new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = newConversation.Id, UserId = userB }
                };

            try
            {
                _db.Conversations.Add(newConversation);
                _db.ConversationParticipants.AddRange(participants);

                // Single SaveChanges makes this atomic
                await _db.SaveChangesAsync();
                return newConversation;
            }
            catch (Exception ex)
            {
              
                return await _db.Conversations
                    .Include(c => c.Participants)
                    .FirstOrDefaultAsync(c =>
                        c.Participants.Count == 2 &&
                        c.Participants.Any(p => p.UserId == userA) &&
                        c.Participants.Any(p => p.UserId == userB)
                    ) ?? throw new Exception("Failed to create or retrieve conversation", ex);
            }
        }


        // Save a direct message
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
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _db.DirectMessages.Add(message);
            await _db.SaveChangesAsync();

            // Re-fetch the message with its navigation properties loaded
            return await _db.DirectMessages
                .Include(m => m.Sender)       // This fixes the "sender" error in your JSON
                .Include(m => m.Conversation) // Optional: if you need conversation details
                .FirstAsync(m => m.Id == message.Id);
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


        //is user a group member check\
        public async Task<bool> IsConversationMemberAsync(
        Guid conversationId,
        Guid userId
        )
        {
            return await _db.ConversationParticipants.AnyAsync(p =>
                p.ConversationId == conversationId &&
                p.UserId == userId
            );
        }


        //Creating group for messaging
        public async Task<NormalResponseDto> CreateUserGroupAsync(
         GroupInputDto input,
         ClaimsPrincipal user
     )
        {
            var userIdClaim =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return new NormalResponseDto
                {
                    StatusCode = 401,
                    Message = "Unauthorized"
                };
            }

            var userId = Guid.Parse(userIdClaim);
            var normalizedGroupTitle = input.Title.Trim().ToLower();

            var isExistingGroup = await _db.Groups
                .AnyAsync(g =>
                    g.UserId == userId &&
                    g.Title.ToLower() == normalizedGroupTitle
                );

            if (isExistingGroup)
            {
                return new NormalResponseDto
                {
                    StatusCode = 409,
                    Message = "Group with the same title already exists"
                };
            }

            // ✅ 1. Create conversation for group chat
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Group,
                CreatedAt= DateTime.UtcNow
            };

            _db.Conversations.Add(conversation);

            // ✅ 2. Create group and link it to conversation
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Title = input.Title,
                Description = input.Description,
                Status = input.Status,
                UserId = userId,
                ConversationId = conversation.Id
            };

            _db.Groups.Add(group);

            // ✅ 3. Add creator as admin participant
            _db.ConversationParticipants.Add(new ConversationParticipant
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                UserId = userId,
                Role = GroupRole.Admin,
                Type = ConversationType.Group,
                JoinedAt = DateTime.UtcNow,

            });

            await _db.SaveChangesAsync();

            return new NormalResponseDto
            {
                StatusCode = 201,
                Message = "Group created successfully"
            };
        }


        //Add member to group for chatting
        public async Task<NormalResponseDto> AddGroupMemberAsync(
        Guid conversationId,
        Guid adminId,
        Guid newUserId
)
        {

            System.Diagnostics.Debug.WriteLine($" conversation Id:{conversationId}");
            System.Diagnostics.Debug.WriteLine($" new Uswer Id:{newUserId}");
            System.Diagnostics.Debug.WriteLine($" admin Id:{adminId}");


            var isAdmin = await _db.ConversationParticipants
                .AnyAsync(p =>
                    p.ConversationId == conversationId &&
                    p.UserId == adminId &&
                    p.Role == GroupRole.Admin
                );

            if (!isAdmin) {
                return new NormalResponseDto
                {
                    Message = "Only admins can add members",
                    StatusCode = 401
                };
        
            }
            _db.ConversationParticipants.Add(new ConversationParticipant
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserId = newUserId,
                JoinedAt = DateTime.UtcNow,
                Role = GroupRole.Member,
                Type = ConversationType.Group,
            });

            await _db.SaveChangesAsync();
            return new NormalResponseDto
            {
                Message = "User added successfully",
                StatusCode = 201,
            };
        }

        // Send message to group
        public async Task<DirectMessage> SendGroupMessageAsync(
        Guid conversationId,
        Guid senderId,
        string content
)
        {
            var isMember = await _db.ConversationParticipants
                .AnyAsync(p =>
                    p.ConversationId == conversationId &&
                    p.UserId == senderId
                );

            if (!isMember)
                throw new UnauthorizedAccessException("User is not a group member");

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


        //Get all participants in a group
        public async Task<NormalResponseWithDataDto<List<ChatUser>>>
        AllGroupParticipants(Guid conversationId)
        {
            var chatUsers = await _db.ConversationParticipants
                .Where(p => p.ConversationId == conversationId)
                .Select(p => new ChatUser
                {
                    Id=p.UserId,
                    Email = p.User.Email,
                    Username = p.User.Username
                })
                .ToListAsync();

            return new NormalResponseWithDataDto<List<ChatUser>>
            {
                Message = "Participants retrieved successfully",
                StatusCode = 200,
                Data = chatUsers
            };
        }

        //Get all groups of a user
        public async Task<NormalResponseWithDataDto<List<Group>>> GetUserGroupsAsync(ClaimsPrincipal user)
        {
            // 1. Get user ID from claims
            var userIdClaim =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return new NormalResponseWithDataDto<List<Group>>
                {
                    Message = "Invalid or missing user identifier",
                    StatusCode = 401,
                    Data = null
                };
            }

            // 2. Get all conversation IDs where user is a participant
            var userConversationIds = await _db.ConversationParticipants
                .Where(p => p.UserId == currentUserId)
                .Select(p => p.ConversationId)
                .ToListAsync();

            // 3. Get groups where user is owner OR participant
            var groups = await _db.Groups
                .Where(g =>
                    g.UserId == currentUserId ||
                    userConversationIds.Contains(g.ConversationId))
                .OrderByDescending(g => g.Status == Enums.GroupStatus.Active)
                .ToListAsync();

            // 4. Return response
            return new NormalResponseWithDataDto<List<Group>>
            {
                Message = "Groups successfully retrieved",
                StatusCode = 200,
                Data = groups
            };
        }
    }
}