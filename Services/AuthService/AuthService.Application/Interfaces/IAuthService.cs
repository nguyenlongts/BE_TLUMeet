namespace AuthService.Application.Interfaces;

using AuthService.Application.DTOs;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request);
    Task<ApiResponse<bool>> ChangePasswordAsync(string userEmail, ChangePasswordRequest request);
    Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
    Task<ApiResponse<bool>> VerifyEmailAsync(VerifyEmailRequest request);
    Task<ApiResponse<bool>> ResendVerificationAsync(ResendVerificationRequest request);
}