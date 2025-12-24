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
                Type = ConversationType.Group
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
                Role = GroupRole.Admin
            });

            await _db.SaveChangesAsync();

            return new NormalResponseDto
            {
                StatusCode = 201,
                Message = "Group created successfully"
            };
        }


        //Add member to group for chatting
        public async Task AddGroupMemberAsync(
        Guid conversationId,
        Guid adminId,
        Guid newUserId
)
        {
            var isAdmin = await _db.ConversationParticipants
                .AnyAsync(p =>
                    p.ConversationId == conversationId &&
                    p.UserId == adminId &&
                    p.Role == GroupRole.Admin
                );

            if (!isAdmin)
                throw new UnauthorizedAccessException("Only admins can add members");

            _db.ConversationParticipants.Add(new ConversationParticipant
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserId = newUserId
            });

            await _db.SaveChangesAsync();
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

            var userIdClaim =
               user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               user.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return new NormalResponseWithDataDto<List<Group>>
                {
                    Message = "User identifier not found",
                    StatusCode = 401,
                    Data = null
                };
            }


    
            var data =  await _db.Groups
                .Where(u=>u.UserId == new Guid(userIdClaim))
                .OrderBy(g => g.Status == Enums.GroupStatus.Active)
                .ToListAsync();

            return new NormalResponseWithDataDto<List<Group>>{
                Message="Groupd successfully retrieved",
                Data=data,
                StatusCode=200
            };
        }
    }
}