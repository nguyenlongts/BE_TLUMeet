using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Interfaces;
using AuthService.Domain.Models;

namespace AuthService.Application.Services.Implements;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(string action, string userEmail, bool isSuccess, string? detail = null, string? ipAddress = null)
    {
        var log = new AuditLog
        {
            Action = action,
            UserEmail = userEmail,
            IsSuccess = isSuccess,
            Detail = detail,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.AuditLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<AuditLogDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var logs = await _unitOfWork.AuditLogs.GetAllAsync(page, pageSize);
        var result = new List<AuditLogDto>();
        foreach (var log in logs)
        {
            result.Add(MapToDto(log));
        }
        return result;
    }

    public async Task<List<AuditLogDto>> GetByEmailAsync(string email, int page = 1, int pageSize = 50)
    {
        var logs = await _unitOfWork.AuditLogs.GetByEmailAsync(email, page, pageSize);
        var result = new List<AuditLogDto>();
        foreach (var log in logs)
        {
            result.Add(MapToDto(log));
        }
        return result;
    }

    private static AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto(log.Id, log.Action, log.UserEmail, log.IsSuccess, log.IpAddress, log.Detail, log.CreatedAt);
    }
}
