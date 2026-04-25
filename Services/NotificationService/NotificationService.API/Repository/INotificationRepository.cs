using NotificationService.API.Model;

namespace NotificationService.API.Repository
{
    public interface INotificationRepository
    {
        Task SaveAsync(Notification notification);
        Task<List<Notification>> GetByEmailAsync(string email);
        Task<Notification?> GetByIdAsync(int id);
        Task UpdateAsync(Notification notification);
        Task UpdateRangeAsync(List<Notification> notifications);
    }
}
