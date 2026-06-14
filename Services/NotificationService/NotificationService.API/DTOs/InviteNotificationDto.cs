namespace NotificationService.API.DTOs
{
    public class InviteNotificationDto
    {
        public int InviteId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string HostEmail { get; set; } = string.Empty;
        public string JoinLink { get; set; } = string.Empty;

        public DateTime? ScheduledDateTime { get; set; }
        public int Duration { get; set; }

        public DateTime ExpiresAt { get; set; }

        // Trạng thái phản hồi của người được mời ("Accepted"/"Declined"); null = chưa phản hồi
        public string? Status { get; set; }
    }
}
