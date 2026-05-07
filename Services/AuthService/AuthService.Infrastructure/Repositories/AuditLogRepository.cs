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

    public async Task LogAsync(AuditLog log)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByUserIdAsync(int userId, int page, int pageSize)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
