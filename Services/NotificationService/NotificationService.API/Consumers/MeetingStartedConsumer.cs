using NotificationService.API;
using NotificationService.API.DTOs;
using NotificationService.API.Events;
using NotificationService.API.Model;
using NotificationService.API.Services;
using System.Text.Json;

public class MeetingStartedConsumer : KafkaConsumerBase<MeetingStartedEvent>
{
    private readonly IServiceProvider _serviceProvider;

    public MeetingStartedConsumer(
        IConfiguration configuration,
        ILogger<MeetingStartedConsumer> logger,
        IServiceProvider serviceProvider) : base(configuration, logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override string Topic => "meeting-started-events";
    protected override string GroupId => "notification-service-meeting-started";

    protected override async Task ProcessMessageAsync(MeetingStartedEvent message)
    {
        using var scope = _serviceProvider.CreateScope();
        var notiService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        // Fallback để không bao giờ hiển thị title rỗng (phòng "")
        var meetingTitle = string.IsNullOrWhiteSpace(message.Title) ? "Cuộc họp" : message.Title;

        var payload = new MeetingStartedNotificationDto
        {
            RoomCode = message.RoomCode,
            Title = meetingTitle,
            HostEmail = message.HostEmail,
            StartedAt = message.StartedAt,
            JoinLink = $"/meet/{message.RoomCode}"
        };

        // Đẩy realtime tới mọi người đang đợi trong phòng (kể cả khách, kể cả khi
        // không có ai được mời) — thay cho việc client polling trạng thái phòng.
        await notiService.SendMeetingStartedToRoomAsync(message.RoomCode, payload);

        // Thông báo "đã bắt đầu" (lưu DB + chuông) chỉ dành cho người được mời đã chấp nhận.
        if (message.AcceptedEmails == null || !message.AcceptedEmails.Any())
            return;

        foreach (var email in message.AcceptedEmails)
        {
            dbContext.Notifications.Add(new Notification
            {
                RecipientEmail = email,
                Type = "MeetingStarted",
                Title = $"Phòng họp \"{meetingTitle}\" đã bắt đầu",
                Payload = JsonSerializer.Serialize(payload),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();

        foreach (var email in message.AcceptedEmails)
        {
            await notiService.SendMeetingStartedAsync(email, payload);
        }
    }
}