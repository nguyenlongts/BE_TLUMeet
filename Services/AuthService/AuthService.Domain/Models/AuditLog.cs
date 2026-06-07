namespace AuthService.Domain.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? IpAddress { get; set; }
    public string? Detail { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
