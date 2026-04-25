namespace NotificationService.API.Model
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; }
    }
}
