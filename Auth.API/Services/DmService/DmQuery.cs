using Auth.API.Data;
using Auth.API.Entities;
using Auth.API.Enums;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using static Auth.API.Services.DmService.DmDTOs;

namespace Auth.API.Services.DmService
{
    [ExtendObjectType("Query")]
    public class DmQuery
    {

        //Fetch direct messages 
        public async Task<List<DirectMessage>> GetMessagesAsync(
         string otherUserId,
         ClaimsPrincipal user,
         AuthDBContext db
 )
        {
            // 1. Identify the logged-in user
            var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                     ?? user.FindFirst("sub")?.Value;


            Guid otherUserGuid = Guid.Parse(otherUserId);

            if (!Guid.TryParse(currentUserIdClaim, out Guid currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            // 2. Find a conversation of type "DM" that contains both users
            var conversation = await db.Conversations
                .Where(c => c.Type == ConversationType.Direct) // Filter by your specific type
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId) &&
                            c.Participants.Any(p => p.UserId == otherUserGuid))
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


        //fetch groups messages 
        public async Task<NormalResponseWithDataDto<List<DirectMessage>>> GetGroupMessagesAsync(
    string conversationId,
    ClaimsPrincipal user,
    AuthDBContext db)
        {
            // 1. Identify the logged-in user
            var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                     ?? user.FindFirst("sub")?.Value;


            System.Diagnostics.Debug.WriteLine($"{conversationId}, conversationId");
            if (!Guid.TryParse(currentUserIdClaim, out Guid currentUserId) ||
                !Guid.TryParse(conversationId, out Guid groupGuid))
            {
                throw new UnauthorizedAccessException("Invalid User or Group ID.");
            }

            // 2. Security Check: Ensure the user belongs to this conversation/group
            var isParticipant = await db.ConversationParticipants
                .AnyAsync(p => p.ConversationId == groupGuid && p.UserId == currentUserId);

            if (!isParticipant)
            {
                throw new UnauthorizedAccessException("You are not a member of this group.");
            }

            // 3. Return the messages for this specific conversation
            var createdMessage = await db.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Conversation) // 🔥 THIS IS THE FIX
            .Where(m => m.ConversationId == groupGuid)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

            return new NormalResponseWithDataDto<List<DirectMessage>>
            {
                Message = "Group message retrieved successfully",
                StatusCode = 200,
                Data = createdMessage!
            };
        }

        [Authorize]
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

