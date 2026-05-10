namespace MeetingService.Infrastructure
{
    public class OutboxService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxService> _logger;
        public OutboxService(IServiceScopeFactory scopeFactory, ILogger<OutboxService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMessage();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi OutboxService");
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            }
        }

        private async Task ProcessMessage()
        {
            using var scoped = _scopeFactory.CreateScope();
            var dbContext = scoped.ServiceProvider.GetRequiredService<MeetingDbContext>();
            var producer = scoped.ServiceProvider.GetRequiredService<IKafkaProducer>();
            var messages = await dbContext.OutboxMessages.Where(m => m.OccuredAt == null).OrderBy(m => m.CreatedAt).Take(20).ToListAsync();
            foreach (var message in messages)
                try
                {
                    switch (message.EventType)
                    {
                        case nameof(MeetingCreatedEvent):
                            var userCreatedEvent = System.Text.Json.JsonSerializer.Deserialize<MeetingCreatedEvent>(message.Payload);
                            await producer.PublishAsync(KafkaTopics.MeetingCreated, userCreatedEvent!);
                            break;
                        case nameof(MeetingStartedEvent):
                            var meetingStartedEvent = System.Text.Json.JsonSerializer.Deserialize<MeetingStartedEvent>(message.Payload);
                            await producer.PublishAsync(KafkaTopics.MeetingStarted, meetingStartedEvent!);
                            break;

                        case nameof(MeetingDeletedEvent):
                            var meetingDeletedEvent = System.Text.Json.JsonSerializer.Deserialize<MeetingDeletedEvent>(message.Payload);
                            await producer.PublishAsync(KafkaTopics.MeetingDeleted, meetingDeletedEvent!);
                            break;
                        case nameof(MeetingEndedEvent):
                            var meetingEndedEvent = System.Text.Json.JsonSerializer.Deserialize<MeetingEndedEvent>(message.Payload);
                            await producer.PublishAsync(KafkaTopics.MeetingEnded, meetingEndedEvent!);
                            break;
                        case nameof(ParticipantJoinedEvent):
                            var participantJoinedEvent = System.Text.Json.JsonSerializer.Deserialize<ParticipantJoinedEvent>(message.Payload);
                            await producer.PublishAsync(KafkaTopics.ParticipantJoined, participantJoinedEvent!);
                            break;
                        case nameof(ParticipantLeftEvent):
                            var participantLeftEvent = System.Text.Json.JsonSerializer.Deserialize<ParticipantLeftEvent>(message.Payload);
                            await producer.PublishAsync(KafkaTopics.ParticipantLeft, participantLeftEvent!);
                            break;
                        case nameof(MeetingInvitedEvent):
                            var meetingInvitedEvent = System.Text.Json.JsonSerializer.Deserialize<MeetingInvitedEvent>(message.Payload);
                            await producer.PublishAsync(KafkaTopics.MeetingInvited, meetingInvitedEvent!);
                            break;
                        case nameof(InviteRespondedEvent):
                            var inviteRespondedEvent = System.Text.Json.JsonSerializer.Deserialize<InviteRespondedEvent>(message.Payload);
                            await producer.PublishAsync(KafkaTopics.InviteResponded, inviteRespondedEvent!);
                            break;
                    }
                    message.OccuredAt = DateTime.UtcNow;
                    message.ErrorMessage = null;

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi publish message {Id}", message.Id);
                    message.ErrorMessage = ex.Message;
                }
            await dbContext.SaveChangesAsync();

        }
    }
}
