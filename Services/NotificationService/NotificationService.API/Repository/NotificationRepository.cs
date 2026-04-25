using Microsoft.EntityFrameworkCore;
using NotificationService.API.Model;

namespace NotificationService.API.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;
        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Notification>> GetByEmailAsync(string email)
        {
            return await _context.Notifications.Where(n => n.RecipientEmail == email).ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.SingleOrDefaultAsync(n => n.NotificationId == id);
        }

        public async Task SaveAsync(Notification notification)
        {
            await _context.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateRangeAsync(List<Notification> notifications)
        {
            _context.Notifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Notification notification)
        {
            var existing = _context.Notifications.SingleOrDefault(n => n.NotificationId == notification.NotificationId);
            if (existing != null)
            {
                existing.RecipientEmail = notification.RecipientEmail;
                existing.Type = notification.Type;
                existing.Title = notification.Title;
                existing.Payload = notification.Payload;
                existing.IsRead = notification.IsRead;
            }
            else
            {
                return;
            }
            await _context.SaveChangesAsync();
        }
    }
}
