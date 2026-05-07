using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAuditLogRepository _auditLog;

    public AdminController(IAuditLogRepository auditLog)
    {
        _auditLog = auditLog;
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var logs = await _auditLog.GetPagedAsync(page, pageSize);
        var result = logs.Select(l => new AuditLogResponse
        {
            Id = l.Id,
            UserId = l.UserId,
            UserEmail = l.UserEmail,
            Action = l.Action,
            Description = l.Description,
            IpAddress = l.IpAddress,
            IsSuccess = l.IsSuccess,
            CreatedAt = l.CreatedAt
        });
        return Ok(ApiResponse<IEnumerable<AuditLogResponse>>.SuccessResponse(result));
    }

    [HttpGet("audit-logs/user/{userId}")]
    public async Task<IActionResult> GetUserAuditLogs(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var logs = await _auditLog.GetByUserIdAsync(userId, page, pageSize);
        var result = logs.Select(l => new AuditLogResponse
        {
            Id = l.Id,
            UserId = l.UserId,
            UserEmail = l.UserEmail,
            Action = l.Action,
            Description = l.Description,
            IpAddress = l.IpAddress,
            IsSuccess = l.IsSuccess,
            CreatedAt = l.CreatedAt
        });
        return Ok(ApiResponse<IEnumerable<AuditLogResponse>>.SuccessResponse(result));
    }
}
