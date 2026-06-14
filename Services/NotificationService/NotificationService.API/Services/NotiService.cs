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
        // Group được tạo bằng email lowercase trong NotificationHub → chuẩn hóa khi gửi để khớp.
        private static string Norm(string email) => (email ?? string.Empty).Trim().ToLowerInvariant();

        public async Task SendInviteAsync(string inviteeEmail, InviteNotificationDto payload)
        {
            await _hubContext.Clients.Group(Norm(inviteeEmail)).SendAsync("ReceiveInvite", payload);
        }

        public async Task SendInviteResponseAsync(string hostEmail, InviteResponseDto payload)
        {
            await _hubContext.Clients.Group(Norm(hostEmail)).SendAsync("ReceiveInviteResponse", payload);
        }
        public async Task SendMeetingStartedAsync(string recipientEmail, MeetingStartedNotificationDto payload)
        {
            await _hubContext.Clients.Group(Norm(recipientEmail))
                .SendAsync("MeetingStarted", payload);
        }

        public async Task SendMeetingStartedToRoomAsync(string roomCode, MeetingStartedNotificationDto payload)
        {
            await _hubContext.Clients.Group(NotificationHub.MeetingGroup(roomCode))
                .SendAsync("MeetingStarted", payload);
        }

        public async Task SendMeetingEndedToRoomAsync(string roomCode, object payload)
        {
            await _hubContext.Clients.Group(NotificationHub.MeetingGroup(roomCode))
                .SendAsync("MeetingEnded", payload);
        }
    }
}
