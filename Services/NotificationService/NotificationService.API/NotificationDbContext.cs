using Microsoft.EntityFrameworkCore;
using NotificationService.API.Model;

namespace NotificationService.API
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<EmailLog> EmailLogs { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(x => x.NotificationId).ValueGeneratedOnAdd();
                entity.Property(e => e.RecipientEmail).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Payload).IsRequired();
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.RecipientEmail);
            });
        }
    }
}
