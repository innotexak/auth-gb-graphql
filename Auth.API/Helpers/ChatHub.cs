using Microsoft.AspNetCore.SignalR;

namespace Auth.API.Helpers
{
   public class ChatHub : Hub
    {
        /* -------------------------
           CONNECTION LIFECYCLE
        --------------------------*/

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            // Optional: add user to a personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        /* -------------------------
           DIRECT MESSAGE (DM)
        --------------------------*/

        public async Task JoinDirectMessage(string otherUserId)
        {
            var currentUserId = Context.UserIdentifier!;
            var dmGroup = GetDmGroupName(currentUserId, otherUserId);

            await Groups.AddToGroupAsync(Context.ConnectionId, dmGroup);
        }

        public async Task SendDirectMessage(string toUserId, string message)
        {
            var fromUserId = Context.UserIdentifier!;
            var dmGroup = GetDmGroupName(fromUserId, toUserId);

            await Clients.Group(dmGroup).SendAsync(
                "ReceiveDirectMessage",
                new
                {
                    fromUserId,
                    toUserId,
                    message,
                    sentAt = DateTime.UtcNow
                }
            );
        }

        public async Task TypingInDirectMessage(string toUserId, bool isTyping)
        {
            var fromUserId = Context.UserIdentifier!;
            var dmGroup = GetDmGroupName(fromUserId, toUserId);

            await Clients
                .Group(dmGroup)
                .SendAsync("UserTyping", fromUserId, isTyping);
        }

        /* -------------------------
           GROUP CHAT
        --------------------------*/

        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetGroupChatName(groupId)
            );
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GetGroupChatName(groupId)
            );
        }

        public async Task SendGroupMessage(string groupId, string message)
        {
            var fromUserId = Context.UserIdentifier!;

            await Clients
                .Group(GetGroupChatName(groupId))
                .SendAsync(
                    "ReceiveGroupMessage",
                    new
                    {
                        groupId,
                        fromUserId,
                        message,
                        sentAt = DateTime.UtcNow
                    }
                );
        }

        public async Task TypingInGroup(string groupId, bool isTyping)
        {
            var fromUserId = Context.UserIdentifier!;

            await Clients
                .Group(GetGroupChatName(groupId))
                .SendAsync(
                    "GroupUserTyping",
                    groupId,
                    fromUserId,
                    isTyping
                );
        }

        /* -------------------------
           HELPERS
        --------------------------*/

        private static string GetDmGroupName(string userA, string userB)
        {
            // Ensures same group name regardless of who starts the chat
            return string.CompareOrdinal(userA, userB) < 0
                ? $"dm:{userA}_{userB}"
                : $"dm:{userB}_{userA}";
        }

        private static string GetGroupChatName(string groupId)
            => $"group:{groupId}";
    }

}
