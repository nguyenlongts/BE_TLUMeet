namespace NotificationService.API.Events;
public class PasswordResetEvent
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
public class UserRegisteredEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public DateTime RegisteredAt { get; set; }
}

public class PasswordChangedEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}

public class MeetingInvitedEvent {
    public int InviteId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;

    public string HostName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string JoinLink { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
public class MeetingStartedEvent
{
    public int MeetingId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public List<string> AcceptedEmails { get; set; } = new();
}
public class InviteRespondedEvent
{
    public int InviteId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}