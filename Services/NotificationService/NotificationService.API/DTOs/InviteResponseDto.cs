namespace NotificationService.API.DTOs
{
    public class InviteResponseDto
    {
        public int InviteId { get; set; }
        public string RoomCode { get; set; } = string.Empty;

        public string InviteeEmail { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
