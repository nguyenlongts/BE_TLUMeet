
using MeetingService.Application.DTOs;
using MeetingService.Domain.Models;
namespace MeetingService.Application.Interfaces;
public interface IMeetingService
{
    Task<ApiResponse<MeetingResponse>> CreateMeetingAsync(CreateMeetingRequest request);
    Task<ApiResponse<MeetingResponse>> GetMeetingByIdAsync(int id);
    Task<ApiResponse<List<MeetingResponse>>> GetAllMeetingsAsync();
    Task<ApiResponse<List<MeetingResponse>>> GetMeetingsByHostEmailAsync(string hostEmail);
    Task<ApiResponse<MeetingResponse>> UpdateMeetingAsync(UpdateMeetingRequest request);
    Task<ApiResponse<bool>> DeleteMeetingAsync(int id);
    Task<ApiResponse<bool>> CheckRoomCodeExistsAsync(string roomCode);

    Task<ApiResponse<MeetingStatusDto>> GetMeetingStatusAsync(string roomCode);
    Task<ApiResponse<bool>> StartMeetingAsync(string roomCode, string hostEmail);
    Task<ApiResponse<bool>> EndMeetingAsync(string roomCode, string hostEmail);
    Task<ApiResponse<bool>> InviteAsync(string roomCode, string hostEmail, List<string> emails);
    Task<ApiResponse<MeetingParticipantResponse>> JoinMeetingAsync(string roomCode, string? userEmail, string? guestName);
    Task<ApiResponse<bool>> LeaveMeetingAsync(string joinToken);
    Task<ApiResponse<List<MeetingParticipantResponse>>> GetParticipantsAsync(string roomCode);
    Task<ApiResponse<MeetingParticipantResponse>> GetParticipantByTokenAsync(string joinToken);

    Task<ApiResponse<bool>> RespondInviteAsync(int inviteId, string inviteeEmail, string status);

}