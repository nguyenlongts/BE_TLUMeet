using NotificationService.API.Events;
using NotificationService.API.Services;

namespace NotificationService.API.Consumers
{
    // Lắng nghe sự kiện kết thúc phòng họp và đẩy realtime tới tất cả người đang
    // ở trong phòng (group "meeting:{roomCode}"), thay cho việc client polling.
    public class MeetingEndedConsumer : KafkaConsumerBase<MeetingEndedEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        public MeetingEndedConsumer(
            IConfiguration configuration,
            ILogger<MeetingEndedConsumer> logger,
            IServiceProvider serviceProvider) : base(configuration, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override string Topic => "meeting-ended-events";
        protected override string GroupId => "notification-service-meeting-ended";

        protected override async Task ProcessMessageAsync(MeetingEndedEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var notiService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await notiService.SendMeetingEndedToRoomAsync(message.RoomCode, new
            {
                roomCode = message.RoomCode,
                endedAt = message.EndedAt
            });
        }
    }
}
