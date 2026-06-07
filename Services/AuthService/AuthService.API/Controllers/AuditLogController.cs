using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _auditLogService.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<List<AuditLogDto>>.SuccessResponse(logs));
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> GetByEmail(string email, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _auditLogService.GetByEmailAsync(email, page, pageSize);
        return Ok(ApiResponse<List<AuditLogDto>>.SuccessResponse(logs));
    }
}
