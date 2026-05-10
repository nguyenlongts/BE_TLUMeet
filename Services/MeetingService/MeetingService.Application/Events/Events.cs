namespace MeetingService.Application.Events;

public class MeetingCreatedEvent
{
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class MeetingStartedEvent
{
    public int MeetingId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public List<string> AcceptedEmails { get; set; } = new();

}
public class MeetingDeletedEvent
{
    public int MeetingId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
public class MeetingEndedEvent
{
    public int MeetingId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;

    public DateTime EndedAt { get; set; }
}

public class ParticipantJoinedEvent
{
    public int ParticipantId { get; set; }
    public int MeetingId { get; set; }
    public string RoomCode { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? UserEmail { get; set; }

    public string ParticipantType { get; set; } = string.Empty; 

    public DateTime JoinedAt { get; set; }
}

public class ParticipantLeftEvent
{
    public int ParticipantId { get; set; }
    public int MeetingId { get; set; }

    public string RoomCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }

    public string ParticipantType { get; set; } = string.Empty;

    public DateTime LeftAt { get; set; }
}

public class MeetingInvitedEvent
{
    public int InviteId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;

    public string HostName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string JoinLink { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
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