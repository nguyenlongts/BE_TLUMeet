using AuthService.Domain.Models;

namespace AuthService.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task LogAsync(AuditLog log);
    Task<List<AuditLog>> GetPagedAsync(int page, int pageSize);
    Task<List<AuditLog>> GetByUserIdAsync(int userId, int page, int pageSize);
}
