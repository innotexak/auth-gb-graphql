using Auth.API.Entities;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using System.Threading;
using static Auth.API.Services.DmService.DmDTOs;

namespace Auth.API.Services.DmService
{
    [ExtendObjectType("Subscription")]
    public class DmSubscription
    {
        [SubscribeAndResolve]
        public async ValueTask<ISourceStream<DirectMessage>> OnMessageSentAsync(
            Guid conversationId,
            [Service] ITopicEventReceiver receiver,
           CancellationToken cancellationToken
        )
        {
            string topic = $"MessageSent_{conversationId}";

            return await receiver.SubscribeAsync<DirectMessage>(topic, cancellationToken);
        }

        [SubscribeAndResolve]
        public async ValueTask<ISourceStream<TypingEvent>> OnUserTypingAsync(
        Guid conversationId,
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    )
        {
            string topic = $"Typing_{conversationId}";
            return await receiver.SubscribeAsync<TypingEvent>(topic, cancellationToken);
        }
    }
}