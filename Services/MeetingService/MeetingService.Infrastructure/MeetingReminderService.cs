using System.Linq;
using MeetingService.Application.Interfaces;

namespace MeetingService.Infrastructure
{
    // Quét định kỳ các cuộc họp sắp diễn ra (trong ReminderLeadMinutes phút tới) chưa gửi nhắc lịch,
    // rồi phát MeetingReminderEvent qua outbox để NotificationService gửi email nhắc.
    public class MeetingReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MeetingReminderService> _logger;
        private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(60);
        private const int ReminderLeadMinutes = 15;

        public MeetingReminderService(IServiceScopeFactory scopeFactory, ILogger<MeetingReminderService> logger)
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
                    await ScanAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi MeetingReminderService");
                }
                await Task.Delay(ScanInterval, stoppingToken);
            }
        }

        private async Task ScanAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var now = DateTime.UtcNow;
            var windowEnd = now.AddMinutes(ReminderLeadMinutes);
            var due = await uow.Meetings.GetDueForReminderAsync(now, windowEnd);
            if (due.Count == 0) return;

            await uow.BeginTransactionAsync();
            try
            {
                foreach (var meeting in due)
                {
                    var accepted = await uow.Invites.GetAcceptedByMeetingIdAsync(meeting.Id);
                    var recipients = new List<string> { meeting.HostEmail }
                        .Concat(accepted.Select(i => i.InviteeEmail))
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Select(e => e.Trim().ToLowerInvariant())
                        .Distinct()
                        .ToList();

                    var evt = new MeetingReminderEvent
                    {
                        MeetingId = meeting.Id,
                        RoomCode = meeting.RoomCode,
                        Title = meeting.Title,
                        HostEmail = meeting.HostEmail,
                        ScheduledDateTime = meeting.ScheduledDateTime!.Value,
                        Duration = meeting.Duration,
                        JoinLink = $"/meet/{meeting.RoomCode}",
                        Recipients = recipients
                    };

                    meeting.ReminderSent = true;
                    await uow.Meetings.UpdateAsync(meeting);
                    await uow.Outbox.AddAsync(new OutboxMessage
                    {
                        EventType = nameof(MeetingReminderEvent),
                        Payload = JsonSerializer.Serialize(evt),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await uow.SaveChangesAsync();
                await uow.CommitAsync();
                _logger.LogInformation("Đã xếp hàng nhắc lịch cho {Count} cuộc họp", due.Count);
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo nhắc lịch");
            }
        }
    }
}
