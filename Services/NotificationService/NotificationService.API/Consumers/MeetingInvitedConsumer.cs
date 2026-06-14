using System.Text.Json;
using NotificationService.API.DTOs;
using NotificationService.API.Events;
using NotificationService.API.Model;
using NotificationService.API.Services;

namespace NotificationService.API.Consumers
{
    public class MeetingInvitedConsumer : KafkaConsumerBase<MeetingInvitedEvent>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        public MeetingInvitedConsumer(IConfiguration configuration, ILogger<MeetingInvitedConsumer> logger, IServiceProvider serviceProvider) : base(configuration, logger)
        {

            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override string Topic => "meeting-invited-events";

        protected override string GroupId => "notification-service-meeting-invited";

        protected override async Task ProcessMessageAsync(MeetingInvitedEvent message)
        {
            using var scope = _serviceProvider.CreateScope();

            var notiService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MeetingInvitedConsumer>>();

            logger.LogInformation($"Processing invite for {message.InviteeEmail}");

            var inviteDTO = new InviteNotificationDto
            {
                InviteId = message.InviteId,
                RoomCode = message.RoomCode,
                HostEmail = message.HostEmail,
                HostName = message.HostName,
                Title = message.Title,
                Description = message.Description,
                JoinLink = message.JoinLink,
                ScheduledDateTime = message.ScheduledDateTime,
                Duration = message.Duration,
                ExpiresAt = message.ExpiresAt
            };

            dbContext.Notifications.Add(new Notification
            {
                RecipientEmail = message.InviteeEmail,
                Type = "MeetingInvite",
                Title = message.Title,
                Payload = JsonSerializer.Serialize(inviteDTO),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            await notiService.SendInviteAsync(message.InviteeEmail, inviteDTO);

            // Gửi email mời tham gia (kèm link vào phòng) — để người được mời biết dù không online
            try
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var fe = _configuration["FE:BaseUrl"];
                var joinLink = string.IsNullOrWhiteSpace(fe)
                    ? message.JoinLink
                    : $"{fe.TrimEnd('/')}/meet/{message.RoomCode}";
                var meetingTitle = string.IsNullOrWhiteSpace(message.Title) ? "Cuộc họp" : message.Title;

                // Chi tiết cuộc họp (chỉ hiển thị phần có dữ liệu)
                var scheduleHtml = message.ScheduledDateTime.HasValue
                    ? $"<p style='margin:4px 0;'>🕒 Thời gian: <strong>{message.ScheduledDateTime.Value:HH:mm dd/MM/yyyy} (UTC)</strong></p>"
                    : "";
                var durationHtml = message.Duration > 0
                    ? $"<p style='margin:4px 0;'>⏱️ Thời lượng: <strong>{message.Duration} phút</strong></p>"
                    : "";
                var descriptionHtml = string.IsNullOrWhiteSpace(message.Description)
                    ? ""
                    : $"<p style='margin:4px 0;'>📝 Mô tả: {message.Description}</p>";

                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #2563eb;'>Bạn được mời tham gia cuộc họp</h2>
                        <p><strong>{message.HostName}</strong> đã mời bạn tham gia cuộc họp <strong>{meetingTitle}</strong> trên TLUMeet.</p>
                        <div style='background:#f3f4f6;border-radius:8px;padding:16px;margin:16px 0;'>
                            <p style='margin:4px 0;'>📌 Tiêu đề: <strong>{meetingTitle}</strong></p>
                            {scheduleHtml}
                            {durationHtml}
                            {descriptionHtml}
                            <p style='margin:4px 0;'>🔑 Mã phòng: <strong>{message.RoomCode}</strong></p>
                        </div>
                        <p style='margin: 28px 0;'>
                            <a href='{joinLink}'
                               style='background-color:#2563eb;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;'>
                                Tham gia cuộc họp
                            </a>
                        </p>
                        <p>Bạn cũng có thể chấp nhận hoặc từ chối lời mời trong phần thông báo trên TLUMeet.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                        <p style='color:#6b7280;font-size:12px;'>
                            Nếu nút không hoạt động, copy link sau vào trình duyệt:<br>{joinLink}
                        </p>
                    </div>";

                await emailService.SendEmailAsync(
                    message.InviteeEmail,
                    $"Lời mời tham gia cuộc họp: {meetingTitle}",
                    emailBody,
                    "MeetingInvite");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Gửi email mời thất bại cho {Email}", message.InviteeEmail);
            }
        }
    }
}
