using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;
namespace NotificationService.API.Hubs
{
    public class NotificationHub :Hub
    {
        public override async Task OnConnectedAsync()
        {
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, email);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, email);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
