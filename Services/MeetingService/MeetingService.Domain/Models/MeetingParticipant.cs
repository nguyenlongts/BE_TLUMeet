using System.Data;

namespace MeetingService.Domain.Models;

public class MeetingParticipant
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string JoinToken { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public Meeting Meeting { get; set; } = null!;
    public Role? Role { get; set; }
}