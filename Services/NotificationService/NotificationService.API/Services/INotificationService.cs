using NotificationService.API.DTOs;

namespace NotificationService.API.Services
{
    public interface INotificationService
    {
        Task SendInviteAsync(string inviteeEmail, InviteNotificationDto payload);
        Task SendInviteResponseAsync(string hostEmail, InviteResponseDto payload);
    }
}
