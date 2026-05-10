using Microsoft.AspNetCore.SignalR;
using NotificationService.API.DTOs;
using NotificationService.API.Hubs;
using NotificationService.API.Repository;

namespace NotificationService.API.Services
{
    public class NotiService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotiService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
         
        }
        public async Task SendInviteAsync(string inviteeEmail, InviteNotificationDto payload)
        {
            await _hubContext.Clients.Group(inviteeEmail).SendAsync("ReceiveInvite", payload);
        }

        public async Task SendInviteResponseAsync(string hostEmail, InviteResponseDto payload)
        {
            await _hubContext.Clients.Group(hostEmail).SendAsync("ReceiveInviteResponse", payload);
        }
        public async Task SendMeetingStartedAsync(string recipientEmail, MeetingStartedNotificationDto payload)
        {
            await _hubContext.Clients.Group(recipientEmail)
                .SendAsync("MeetingStarted", payload);
        }
    }
}
