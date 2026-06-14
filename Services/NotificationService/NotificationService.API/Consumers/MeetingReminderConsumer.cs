using System.Text.Json;
using NotificationService.API.Events;
using NotificationService.API.Model;
using NotificationService.API.Services;

namespace NotificationService.API.Consumers
{
    // Gửi email nhắc lịch + lưu thông báo cho host và những người đã chấp nhận, trước giờ họp.
    public class MeetingReminderConsumer : KafkaConsumerBase<MeetingReminderEvent>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        protected override string Topic => "meeting-reminder-events";
        protected override string GroupId => "notification-service-meeting-reminder";

        public MeetingReminderConsumer(IConfiguration configuration, ILogger<MeetingReminderConsumer> logger, IServiceProvider serviceProvider) : base(configuration, logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ProcessMessageAsync(MeetingReminderEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MeetingReminderConsumer>>();

            var fe = _configuration["FE:BaseUrl"];
            var joinLink = string.IsNullOrWhiteSpace(fe)
                ? message.JoinLink
                : $"{fe.TrimEnd('/')}/meet/{message.RoomCode}";
            var title = string.IsNullOrWhiteSpace(message.Title) ? "Cuộc họp" : message.Title;
            var localTime = message.ScheduledDateTime.AddHours(7); // GMT+7

            // Payload cho chuông (camelCase để FE NotiDetail đọc được)
            var payload = JsonSerializer.Serialize(new
            {
                roomCode = message.RoomCode,
                title,
                scheduledDateTime = message.ScheduledDateTime,
                duration = message.Duration,
                joinLink
            });

            foreach (var email in message.Recipients ?? new List<string>())
            {
                if (string.IsNullOrWhiteSpace(email)) continue;

                dbContext.Notifications.Add(new Notification
                {
                    RecipientEmail = email,
                    Type = "MeetingReminder",
                    Title = $"Sắp diễn ra: {title}",
                    Payload = payload,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                try
                {
                    var emailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #2563eb;'>Nhắc lịch họp</h2>
                            <p>Cuộc họp <strong>{title}</strong> sắp bắt đầu.</p>
                            <div style='background:#f3f4f6;border-radius:8px;padding:16px;margin:16px 0;'>
                                <p style='margin:4px 0;'>🕒 Thời gian: <strong>{localTime:HH:mm dd/MM/yyyy} (GMT+7)</strong></p>
                                <p style='margin:4px 0;'>⏱️ Thời lượng: <strong>{message.Duration} phút</strong></p>
                                <p style='margin:4px 0;'>🔑 Mã phòng: <strong>{message.RoomCode}</strong></p>
                            </div>
                            <p style='margin: 28px 0;'>
                                <a href='{joinLink}'
                                   style='background-color:#2563eb;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;'>
                                    Vào phòng họp
                                </a>
                            </p>
                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                            <p style='color:#6b7280;font-size:12px;'>
                                Nếu nút không hoạt động, copy link sau vào trình duyệt:<br>{joinLink}
                            </p>
                        </div>";

                    await emailService.SendEmailAsync(
                        email,
                        $"Nhắc lịch họp: {title}",
                        emailBody,
                        "MeetingReminder");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Gửi email nhắc lịch thất bại cho {Email}", email);
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
