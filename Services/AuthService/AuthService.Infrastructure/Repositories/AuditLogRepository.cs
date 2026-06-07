using AuthService.Domain.Interfaces;
using AuthService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AuthDbContext _context;

    public AuditLogRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
    }

    public async Task<List<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByEmailAsync(string email, int page = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(a => a.UserEmail == email)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
