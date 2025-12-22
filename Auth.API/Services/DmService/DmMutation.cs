using Auth.API.Entities;
using HotChocolate.Subscriptions;
using System.Security.Claims;
using static Auth.API.Services.DmService.DmDTOs;

namespace Auth.API.Services.DmService
{
    [ExtendObjectType(Name = "Mutation")]
    public class DmMutation
    {
        public async Task<DirectMessage> SendDirectMessageAsync(
            string receiverId,
            string message,
            ClaimsPrincipal user,
            [Service] DmDatasource datasource,
            [Service] ITopicEventSender eventSender
        )
        {
            var senderIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(senderIdClaim))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            Guid senderId = Guid.Parse(senderIdClaim);
            Guid rId = Guid.Parse(receiverId);

            var conversation = await datasource.GetOrCreateConversationAsync(
                senderId,
                rId
            );

            var dm = await datasource.SaveMessageAsync(
                conversation.Id,
                senderId,
                message
            );
            System.Diagnostics.Debug.WriteLine($"conversationId {conversation.Id}");
            await eventSender.SendAsync(
                $"MessageSent_{conversation.Id}",
                dm
            );

            return dm;
        }

       public async Task<bool> SetTypingAsync(
           Guid conversationId,
           bool isTyping,
           ClaimsPrincipal user,
           [Service] ITopicEventSender eventSender
       )
       {
                var userIdClaim =
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    user.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    throw new UnauthorizedAccessException();
                }

                var typingEvent = new TypingEvent
                {
                    ConversationId = conversationId,
                    UserId = Guid.Parse(userIdClaim),
                    IsTyping = isTyping
                };

                await eventSender.SendAsync(
                    $"Typing_{conversationId}",
                    typingEvent
                );

                return true;
       }
    }
}
