using NotificationService.API.DTOs;
using NotificationService.API.Events;
using NotificationService.API.Model;
using NotificationService.API.Services;
using System.Text.Json;

namespace NotificationService.API.Consumers
{
    public class InviteRespondedConsumer : KafkaConsumerBase<InviteRespondedEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        protected override string GroupId => "notification-service-invite-responded";
        public InviteRespondedConsumer(IConfiguration configuration, ILogger<InviteRespondedConsumer> logger, IServiceProvider serviceProvider) : base(configuration, logger)
        {
            _serviceProvider = serviceProvider;

        }

        protected override string Topic => "invite-responded-events";

        

        protected override async Task ProcessMessageAsync(InviteRespondedEvent message)
        {
            using var scoped = _serviceProvider.CreateScope();
            var notiService = scoped.ServiceProvider.GetRequiredService<INotificationService>();
            var dbcontext = scoped.ServiceProvider.GetRequiredService<NotificationDbContext>();
            var responsedDTO = new InviteResponseDto
            {
               InviteeEmail = message.InviteeEmail,
               InviteId = message.InviteId,
                RoomCode = message.RoomCode,
                Status = message.Status
            }; 
            await dbcontext.Notifications.AddAsync(new Notification
            {
                RecipientEmail = message.HostEmail,
                Type = "MeetingInviteResponse",
                Title = message.Title,
                Payload = JsonSerializer.Serialize(responsedDTO),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await dbcontext.SaveChangesAsync();
            await notiService.SendInviteResponseAsync(message.HostEmail,responsedDTO);
        }
    }
}
