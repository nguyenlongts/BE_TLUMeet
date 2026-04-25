using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Model;

namespace NotificationService.API.Services
{
    public interface INotificationAppService
    {
        Task<List<Notification>> GetByEmailAsync(string email);
        Task<bool> MarkAsReadAsync(int id, string email);
        Task MarkAllAsReadAsync(string email);
    }
}
