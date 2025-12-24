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
         
                await eventSender.SendAsync(
                    $"MessageSent_{conversation.Id}",
                    dm
                );

                return dm;
            }


            public async Task<DirectMessage> SendGroupMessageAsync(
                Guid conversationId,
                string message,
                ClaimsPrincipal user,
                [Service] DmDatasource datasource,
                [Service] ITopicEventSender eventSender
            )
            {
                var senderIdClaim =
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    user.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(senderIdClaim))
                    throw new UnauthorizedAccessException("User not authenticated.");

                var senderId = Guid.Parse(senderIdClaim);

                // ✅ 1. Verify user is a group member
                var isMember = await datasource.IsConversationMemberAsync(
                    conversationId,
                    senderId
                );

                if (!isMember)
                    throw new GraphQLException("You are not a member of this group.");

                // ✅ 2. Save message
                var messageEntity = await datasource.SendGroupMessageAsync(
                    conversationId,
                    senderId,
                    message
                );

                // ✅ 3. Publish to group subscribers
                await eventSender.SendAsync(
                    $"MessageSent_{conversationId}",
                    messageEntity
                );

                return messageEntity;
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


            public async Task<NormalResponseDto> CreateUserGroup(
                GroupInputDto input,
                ClaimsPrincipal user,
                [Service] DmDatasource datasource
                )
            {
             return await datasource.CreateUserGroupAsync(input, user);

            }


            //Add user to group
            public async Task<NormalResponseDto> AddUserToGroupChat(
                Guid conversationId, 
                Guid newUserId, 
                ClaimsPrincipal user,
                [Service] DmDatasource datasource)
            {

                var adminId =
                        user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        user.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(adminId))
                {
                    throw new GraphQLException("You must login first for this action");

                }
                await datasource.AddGroupMemberAsync(conversationId, new Guid(adminId), newUserId);

                return new NormalResponseDto
                {
                    Message = "Memeber added successfully",
                    StatusCode = 201
                }; 
            }

         
        }
    }
