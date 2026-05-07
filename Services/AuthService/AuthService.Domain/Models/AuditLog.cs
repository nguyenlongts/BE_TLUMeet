namespace AuthService.Domain.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = null!;
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
