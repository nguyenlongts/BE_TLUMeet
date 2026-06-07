using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IAuditLogService auditLogService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _auditLogService.LogAsync("Register", request.Email, result.Success, result.Success ? null : result.Message, ip);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _auditLogService.LogAsync("Login", request.Email, result.Success, result.Success ? null : result.Message, ip);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userEmail = User.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized(new { message = "Invalid token" });

        var result = await _authService.ChangePasswordAsync(userEmail, request);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _auditLogService.LogAsync("ChangePassword", userEmail, result.Success, result.Success ? null : result.Message, ip);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _auditLogService.LogAsync("ForgotPassword", request.Email, result.Success, result.Success ? null : result.Message, ip);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _auditLogService.LogAsync("ResetPassword", request.Token, result.Success, result.Success ? null : result.Message, ip);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst("userId")?.Value;
        var name = User.FindFirst("name")?.Value;
        var email = User.FindFirst("email")?.Value;
        var role = User.FindFirst("role")?.Value;

        return Ok(new
        {
            userId,
            name,
            email,
            role
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.LogoutAsync(request.RefreshToken);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);
        return Ok(result);
    }
}
