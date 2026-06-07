using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Interfaces;
using AuthService.Domain.Models;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Claims;
namespace AuthService.Application.Services.Implements;

public class AuthServiceImpl : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthServiceImpl> _logger;
    public AuthServiceImpl(IConfiguration configuration, ILogger<AuthServiceImpl> logger, IUnitOfWork uow)
    {

        _configuration = configuration;
        _logger = logger;
        _unitOfWork = uow;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {

        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<AuthResponse>.ErrorResponse(400, "Tên không được để trống");

        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<AuthResponse>.ErrorResponse(400, "Email không được để trống");

        if (!IsValidEmail(request.Email))
            return ApiResponse<AuthResponse>.ErrorResponse(400, "Email không hợp lệ");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return ApiResponse<AuthResponse>.ErrorResponse(400, "Mật khẩu phải có ít nhất 6 ký tự");

        var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return ApiResponse<AuthResponse>.ErrorResponse(400, "Email đã được sử dụng");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            UserName = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };


        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var createdUser = await _unitOfWork.Users.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var refreshToken = CreateNewRefreshToken(user.Id);
            await _unitOfWork.RefreshTokens.SaveAsync(refreshToken);
            var outboxMessage = new OutboxMessage
            {
                EventType = nameof(UserRegisteredEvent),
                Payload = JsonSerializer.Serialize(new UserRegisteredEvent
                {
                    UserId = createdUser.Id,
                    Email = createdUser.Email,
                    UserName = createdUser.UserName,
                    RegisteredAt = DateTime.UtcNow,
                }),
                CreatedAt = DateTime.UtcNow,
                ErrorMessage = null
            };

            await _unitOfWork.OutboxMessages.AddAsync(outboxMessage);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            var accessToken = GenerateJwtToken(createdUser);

            var response = new AuthResponse
            {
                Id = createdUser.Id,
                Name = createdUser.UserName,
                Email = createdUser.Email,
                Token = accessToken,
                RefreshToken = refreshToken.Token.ToString()
            };
            return ApiResponse<AuthResponse>.SuccessResponse(response, "Đăng ký thành công");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Lỗi transaction RegisterAsync");
            return ApiResponse<AuthResponse>.ErrorResponse(500, "Lỗi server, vui lòng thử lại sau");
        }


    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {

        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<AuthResponse>.ErrorResponse(400, "Email không được để trống");

        if (string.IsNullOrWhiteSpace(request.Password))
            return ApiResponse<AuthResponse>.ErrorResponse(400, "Mật khẩu không được để trống");

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email.ToLower());
        if (user == null)
            return ApiResponse<AuthResponse>.ErrorResponse(401, "Email hoặc mật khẩu không chính xác");

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.ErrorResponse(403, "Tài khoản đã bị vô hiệu hóa");

        var isLocked = await _unitOfWork.Users.IsAccountLockedAsync(user);
        if (isLocked && user.LoginInfo?.AccountLockedUntil.HasValue == true)
        {
            var remainingMinutes = (int)Math.Ceiling(
                (user.LoginInfo.AccountLockedUntil.Value - DateTime.UtcNow).TotalMinutes);
            return ApiResponse<AuthResponse>.ErrorResponse(403,
                $"Tài khoản bị khóa. Thử lại sau {remainingMinutes} phút");
        }

        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            await _unitOfWork.Users.IncrementFailedLoginAttemptsAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var isLockedAfterFail = await _unitOfWork.Users.IsAccountLockedAsync(user);
            if (isLockedAfterFail)
                return ApiResponse<AuthResponse>.ErrorResponse(403,
                    "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần");

            return ApiResponse<AuthResponse>.ErrorResponse(401, "Email hoặc mật khẩu không chính xác");
        }

        var refreshToken = Guid.NewGuid();
        try
        {
            var refreshTokenExpDays = int.Parse(_configuration["Jwt:RefreshTokenExpiration"] ?? "7");
            await _unitOfWork.BeginTransactionAsync();
            if (user.LoginInfo != null)
            {
                user.LoginInfo.FailedLoginAttempts = 0;
                user.LoginInfo.AccountLockedUntil = null;
                user.LoginInfo.LastLogin = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
            }
            await _unitOfWork.RefreshTokens.RevokeAllByUserIdAsync(user.Id);

            await _unitOfWork.RefreshTokens.SaveAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiredAt = DateTime.UtcNow.AddDays(refreshTokenExpDays),
                CreatedAt = DateTime.UtcNow,
                RevokeAt = null
            });
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Lỗi transaction Login");
            return ApiResponse<AuthResponse>.ErrorResponse(500, "Lỗi server, vui lòng thử lại sau");
        }

        var token = GenerateJwtToken(user);

        var response = new AuthResponse
        {
            Id = user.Id,
            Name = user.UserName,
            Email = user.Email,
            Token = token,
            RefreshToken = refreshToken.ToString()
        };

        return ApiResponse<AuthResponse>.SuccessResponse(response, "Đăng nhập thành công");

    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(string userEmail, ChangePasswordRequest request)
    {

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return ApiResponse<bool>.ErrorResponse(400, "Mật khẩu hiện tại không được để trống");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return ApiResponse<bool>.ErrorResponse(400, "Mật khẩu mới phải có ít nhất 6 ký tự");
        if (request.CurrentPassword == request.NewPassword)
            return ApiResponse<bool>.ErrorResponse(400, "Mật khẩu mới không được trùng mật khẩu cũ");
        var user = await _unitOfWork.Users.GetByEmailAsync(userEmail);
        if (user == null)
            return ApiResponse<bool>.ErrorResponse(404, "Người dùng không tồn tại");

        if (!user.IsActive)
            return ApiResponse<bool>.ErrorResponse(403, "Tài khoản đã bị vô hiệu hóa");

        var isValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isValid)
            return ApiResponse<bool>.ErrorResponse(400, "Mật khẩu hiện tại không đúng");

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.RefreshTokens.RevokeAllByUserIdAsync(user.Id);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi transaction Change Password");
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.ErrorResponse(500, "Lỗi server, vui lòng thử lại sau");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Đổi mật khẩu thành công");

    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {

        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<bool>.ErrorResponse(400, "Email không được để trống");

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user == null)
            return ApiResponse<bool>.SuccessResponse(true, "Nếu email tồn tại, link reset đã được gửi");

        var resetToken = GenerateResetPasswordToken(user);
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(15);
        var passwordResetEvent = new PasswordResetRequestedEvent
        {
            Email = request.Email,
            ResetToken = resetToken,
            RequestedAt = now,
            ExpiresAt = expiresAt
        };

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.Users.DeleteUnusedResetTokensAsync(user.Id);

            await _unitOfWork.Users.SaveResetTokenAsync(new PasswordResetToken
            {
                UserId = user.Id,
                Token = resetToken,
                ExpiresAt = expiresAt
            });
            var outboxMessage = new OutboxMessage
            {
                EventType = nameof(PasswordResetRequestedEvent),
                Payload = JsonSerializer.Serialize(passwordResetEvent),
                CreatedAt = now,
                ErrorMessage = null
            };
            await _unitOfWork.OutboxMessages.AddAsync(outboxMessage);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi transaction ForgotPasswordAsync");
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.ErrorResponse(500, "Lỗi server, vui lòng thử lại sau");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Đã gửi email đặt lại mật khẩu");

    }
    private static string PadBase64(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
    }
    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {

        if (string.IsNullOrWhiteSpace(request.Token))
            return ApiResponse<bool>.ErrorResponse(400, "Token không hợp lệ");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return ApiResponse<bool>.ErrorResponse(400, "Mật khẩu mới tối thiểu 6 ký tự");

        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();
        var rawToken = handler.ReadJwtToken(request.Token);

        try
        {
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
            };
            handler.ValidateToken(request.Token, validationParams, out _); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT validation failed");
            return ApiResponse<bool>.ErrorResponse(400, "Token không hợp lệ");
        }
        var parts = request.Token.Split('.');
        if (parts.Length != 3)
            return ApiResponse<bool>.ErrorResponse(400, "Token không hợp lệ");

        var payloadJson = Encoding.UTF8.GetString(
            Convert.FromBase64String(PadBase64(parts[1])));

        var payloadDoc = JsonDocument.Parse(payloadJson);
        var purpose = payloadDoc.RootElement.TryGetProperty("purpose", out var p)? p.GetString() : null;
        var userEmail = payloadDoc.RootElement.TryGetProperty("email", out var e)? e.GetString() : null;
        var exp = payloadDoc.RootElement.TryGetProperty("exp", out var expVal)? expVal.GetInt64() : 0;
      

        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp)
            return ApiResponse<bool>.ErrorResponse(400, "Token đã hết hạn");

        if (purpose != "reset-password")
            return ApiResponse<bool>.ErrorResponse(400, "Token không hợp lệ");

        var savedToken = await _unitOfWork.Users.GetResetTokenAsync(request.Token);
        if (savedToken == null || savedToken.IsUsed || savedToken.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<bool>.ErrorResponse(400, "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn");

        var user = await _unitOfWork.Users.GetByEmailAsync(userEmail);

        if (user == null)
            return ApiResponse<bool>.ErrorResponse(404, "Người dùng không tồn tại");


        try
        {
            await _unitOfWork.BeginTransactionAsync();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            savedToken.IsUsed = true;
            await _unitOfWork.Users.ResetFailedLoginAttemptsAsync(user);

            await _unitOfWork.RefreshTokens.RevokeAllByUserIdAsync(user.Id);
            await _unitOfWork.Users.UpdateResetTokenAsync(savedToken);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi transaction Reset Password");
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.ErrorResponse(500, "Lỗi server, vui lòng thử lại sau");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Đặt lại mật khẩu thành công");
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "1440");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("name", user.UserName),
            new Claim("email", user.Email),
            new Claim("role", user.Role?.Name ?? "User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateResetPasswordToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var expirationMinutes = 15;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("email", user.Email),
            new Claim("purpose", "reset-password"),
        };
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);

    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        var existingToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if (existingToken == null || existingToken.ExpiredAt < DateTime.UtcNow || existingToken.RevokeAt != null)
            return ApiResponse<AuthResponse>.ErrorResponse(400, "Refresh token không hợp lệ hoặc đã hết hạn");
        var user = await _unitOfWork.Users.GetByIdAsync(existingToken.UserId);
        if (user == null)
            return ApiResponse<AuthResponse>.ErrorResponse(401, "Người dùng không tồn tại");

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.ErrorResponse(403, "Tài khoản đã bị vô hiệu hóa");

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.RefreshTokens.RevokeAsync(existingToken.Token.ToString());
            var newRefreshToken = CreateNewRefreshToken(existingToken.UserId);
            await _unitOfWork.RefreshTokens.SaveAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            var accessToken = GenerateJwtToken(user);
            var response = new AuthResponse
            {
                Id = user.Id,
                Name = user.UserName,
                Email = user.Email,
                Token = accessToken,
                RefreshToken = newRefreshToken.Token.ToString()
            };
            return ApiResponse<AuthResponse>.SuccessResponse(response, "Làm mới token thành công");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Lỗi Refresh Token");
            return ApiResponse<AuthResponse>.ErrorResponse(500, "Lỗi server, vui lòng thử lại sau");
        }

    }
    private RefreshToken CreateNewRefreshToken(int userId)
    {
        var expirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpiration"] ?? "7");
        var token = new RefreshToken
        {
            UserId = userId,
            Token = Guid.NewGuid(),
            ExpiredAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow,
            RevokeAt = null
        };
        return token;
    }
    public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
    {
        try
        {
            var existToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
            if (existToken != null && existToken.RevokeAt == null)
            {
                await _unitOfWork.RefreshTokens.RevokeAsync(existToken.Token.ToString());
                await _unitOfWork.SaveChangesAsync();

            }
            return ApiResponse<bool>.SuccessResponse(true, "Đăng xuất thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi Logout");

            return ApiResponse<bool>.ErrorResponse(500, "Lỗi server, vui lòng thử lại sau");
        }
    }
}