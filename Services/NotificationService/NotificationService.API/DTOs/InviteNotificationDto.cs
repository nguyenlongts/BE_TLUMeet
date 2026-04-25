namespace NotificationService.API.DTOs
{
    public class InviteNotificationDto
    {
        public int InviteId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string HostEmail { get; set; } = string.Empty;   
        public string JoinLink { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }  
    }
}
