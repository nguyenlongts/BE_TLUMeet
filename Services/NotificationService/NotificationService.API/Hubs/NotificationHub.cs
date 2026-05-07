using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var email = Context.User!.FindFirst("email")!.Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, email);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var email = Context.User!.FindFirst("email")!.Value;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, email);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
