namespace AuthService.Infrastructure.Messaging;

public class UserRegisteredEvent
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}

public class UserLoggedInEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime LoggedInAt { get; set; }
}

public class PasswordChangedEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}

public class WelcomeEmailEvent
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class PasswordResetRequestedEvent
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class EmailVerificationRequestedEvent
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string VerificationToken { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

