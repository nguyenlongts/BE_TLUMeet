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
        public MeetingInvitedConsumer(IConfiguration configuration, ILogger<MeetingInvitedConsumer> logger, IServiceProvider serviceProvider) : base(configuration, logger)
        {

            _serviceProvider = serviceProvider;
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
                JoinLink = message.JoinLink,
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
        }
    }
}
