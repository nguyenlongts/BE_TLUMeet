namespace NotificationService.API.DTOs
{
    public class MeetingStartedNotificationDto
    {
        public string RoomCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string HostEmail { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public string JoinLink { get; set; } = string.Empty;
    }
}
