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
        if (message.AcceptedEmails == null || !message.AcceptedEmails.Any())
            return;

        using var scope = _serviceProvider.CreateScope();
        var notiService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var payload = new MeetingStartedNotificationDto
        {
            RoomCode = message.RoomCode,
            Title = message.Title,
            HostEmail = message.HostEmail,
            StartedAt = message.StartedAt,
            JoinLink = $"/meet/{message.RoomCode}"
        };

        foreach (var email in message.AcceptedEmails)
        {
            dbContext.Notifications.Add(new Notification
            {
                RecipientEmail = email,
                Type = "MeetingStarted",
                Title = $"Phòng họp \"{message.Title}\" đã bắt đầu",
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