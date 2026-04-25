using MeetingService.Domain.Enums;

namespace MeetingService.Application.DTOs;

public class CreateMeetingRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string HostEmail { get; set; } = string.Empty;
    public bool RequireHostToStart { get; set; }
    public DateTime? ScheduledDateTime { get; set; }

    public int Duration { get; set; }
}

public class UpdateMeetingRequest
{
    public string RoomCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ScheduledDateTime { get; set; }
    public string HostEmail { get; set; } = string.Empty;
    public int Duration { get; set; }
}

public class MeetingResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public DateTime? ScheduledDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string MeetingLink { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class JoinMeetingRequest
{
    public string? UserEmail { get; set; }
    public string? GuestName { get; set; }
}

public class MeetingParticipantResponse
{
    public int ParticipantId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string JoinToken { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}

public class MeetingStatusDto
{
    public string RoomCode { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public bool RequireHostToStart { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsStarted { get; set; }
    public bool IsEnded { get; set; }

}

public class StartMeetingRequest
{
    public string HostEmail { get; set; } = string.Empty;
}

public class EndMeetingRequest
{
    public string HostEmail { get; set; } = string.Empty;
}

public class LeaveMeetingRequest
{
    public string JoinToken { get; set; } = string.Empty;
}
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    public static ApiResponse<T> ErrorResponse(int statusCode, string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode
        };
    }
}
public class InviteRequest
{
    public List<string> InviteeEmails { get; set; } = new();
}

public class InviteRespondRequest
{
    public string Status { get; set; } = string.Empty;
}