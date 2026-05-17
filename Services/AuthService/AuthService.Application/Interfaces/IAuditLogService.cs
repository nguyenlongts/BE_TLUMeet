using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string action, string userEmail, bool isSuccess, string? detail = null, string? ipAddress = null);
    Task<List<AuditLogDto>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<List<AuditLogDto>> GetByEmailAsync(string email, int page = 1, int pageSize = 50);
}
