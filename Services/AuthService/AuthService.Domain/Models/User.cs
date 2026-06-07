namespace AuthService.Domain.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? RoleId { get; set; }
    public Role? Role { get; set; }
    public UserLogin? LoginInfo { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();

}