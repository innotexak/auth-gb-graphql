using Auth.API.Data;
using Auth.API.Entities;
using Auth.API.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using static Auth.API.Services.DmService.DmDTOs;

namespace Auth.API.Services.DmService
{
    [ExtendObjectType(Name = "Query")]
    public class DmQuery
    {
        public async Task<List<DirectMessage>> GetMessagesAsync(
         string otherUserId,
         ClaimsPrincipal user,
         AuthDBContext db
 )
        {
            // 1. Identify the logged-in user
            var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                     ?? user.FindFirst("sub")?.Value;
           

            if (!Guid.TryParse(currentUserIdClaim, out Guid currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            // 2. Find a conversation of type "DM" that contains both users
            var conversation = await db.Conversations
                .Where(c => c.Type == ConversationType.Direct) // Filter by your specific type
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId) &&
                            c.Participants.Any(p => p.UserId == new Guid(otherUserId)))
                .Select(c => new { c.Id })
                .FirstOrDefaultAsync();

            // 3. If no conversation exists yet, return empty list
            if (conversation == null)
            {
                return new List<DirectMessage>();
            }

            // 4. Return the messages
            return await db.DirectMessages
                .Where(m => m.ConversationId == conversation.Id)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<NormalResponseWithDataDto<List<Group>>> GetUserGroups(
        ClaimsPrincipal user,
        [Service] DmDatasource datasource
        )
        {
            return await datasource.GetUserGroupsAsync(user);

        }

        public async Task<NormalResponseWithDataDto<List<ChatUser>>> GetGroupParticipants(
            Guid conversationId,
            [Service] DmDatasource datasource
         )
        {
            return await datasource.AllGroupParticipants(conversationId);
        }
    }
}

