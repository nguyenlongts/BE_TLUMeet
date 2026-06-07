using AuthService.Domain.Models;

namespace AuthService.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<List<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<List<AuditLog>> GetByEmailAsync(string email, int page = 1, int pageSize = 50);
}
