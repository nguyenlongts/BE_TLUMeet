using MeetingService.Domain.Enums;

namespace MeetingService.Domain.Models;

public class Meeting
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;

    public DateTime? ScheduledDateTime { get; set; }

    public int Duration { get; set; }

    public bool RequireHostToStart { get; set; } = true;

    public DateTime? ActualStartTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;

    // Đã gửi nhắc lịch trước giờ họp hay chưa (tránh gửi trùng)
    public bool ReminderSent { get; set; } = false;


    public ICollection<MeetingParticipant> Participants { get; set; }
        = new List<MeetingParticipant>();
}