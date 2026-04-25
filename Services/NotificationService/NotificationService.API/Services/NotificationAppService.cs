using NotificationService.API.Model;
using NotificationService.API.Repository;

namespace NotificationService.API.Services
{
    public class NotificationAppService : INotificationAppService
    {
        private readonly INotificationRepository _repo;

        public NotificationAppService(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Notification>> GetByEmailAsync(string email)
        {
            return await _repo.GetByEmailAsync(email);
        }

        public async Task<bool> MarkAsReadAsync(int id, string email)
        {
            var notification = await _repo.GetByIdAsync(id);

            if (notification == null || notification.RecipientEmail != email)
                return false;

            notification.IsRead = true;
            await _repo.UpdateAsync(notification);
            return true;
        }

        public async Task MarkAllAsReadAsync(string email)
        {
            var notifications = await _repo.GetByEmailAsync(email);

            var unread = notifications.Where(n => !n.IsRead).ToList();
            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _repo.UpdateRangeAsync(unread);
        }
    }
}