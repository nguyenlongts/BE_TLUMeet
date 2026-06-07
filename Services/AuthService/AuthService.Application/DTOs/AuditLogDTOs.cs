namespace AuthService.Application.DTOs;

public record AuditLogDto(
    int Id,
    string Action,
    string UserEmail,
    bool IsSuccess,
    string? IpAddress,
    string? Detail,
    DateTime CreatedAt
);
