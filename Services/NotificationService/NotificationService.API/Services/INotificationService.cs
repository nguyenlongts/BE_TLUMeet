using NotificationService.API.DTOs;

namespace NotificationService.API.Services
{
    public interface INotificationService
    {
        Task SendInviteAsync(string inviteeEmail, InviteNotificationDto payload);
        Task SendInviteResponseAsync(string hostEmail, InviteResponseDto payload);
        Task SendMeetingStartedAsync(string recipientEmail, MeetingStartedNotificationDto payload);

        // Đẩy sự kiện vòng đời tới tất cả người đang ở trong phòng (theo roomCode).
        Task SendMeetingStartedToRoomAsync(string roomCode, MeetingStartedNotificationDto payload);
        Task SendMeetingEndedToRoomAsync(string roomCode, object payload);
    }
}
