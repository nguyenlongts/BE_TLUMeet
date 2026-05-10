using MeetingService.Application.DTOs;
using MeetingService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeetingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeetingController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public MeetingController(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequest request)
    {
        var result = await _meetingService.CreateMeetingAsync(request);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMeeting(int id)
    {
        var result = await _meetingService.GetMeetingByIdAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMeetings()
    {
        var result = await _meetingService.GetAllMeetingsAsync();
        return Ok(result);
    }

    [HttpGet("host/{hostEmail}")]
    public async Task<IActionResult> GetMeetingsByHost(string hostEmail)
    {
        var result = await _meetingService.GetMeetingsByHostEmailAsync(hostEmail);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMeeting([FromBody] UpdateMeetingRequest request)
    {
        var result = await _meetingService.UpdateMeetingAsync(request);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _meetingService.DeleteMeetingAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("check/{roomCode}")]
    public async Task<IActionResult> CheckRoomCode(string roomCode)
    {
        var result = await _meetingService.CheckRoomCodeExistsAsync(roomCode);
        return Ok(result);
    }

    [HttpGet("{roomCode}/status")]
    public async Task<IActionResult> GetMeetingStatus(string roomCode)
    {
        var result = await _meetingService.GetMeetingStatusAsync(roomCode);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
    [Authorize]
    [HttpPost("{roomCode}/start")]
    public async Task<IActionResult> StartMeeting(string roomCode)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized();
        var result = await _meetingService.StartMeetingAsync(roomCode, email);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
    [Authorize]
    [HttpPost("{roomCode}/end")]
    public async Task<IActionResult> EndMeeting(string roomCode)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var result = await _meetingService.EndMeetingAsync(roomCode, email);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("{roomCode}/join")]
    public async Task<IActionResult> JoinMeeting(string roomCode, [FromBody] JoinMeetingRequest request)
    {
        var result = await _meetingService.JoinMeetingAsync(
            roomCode,
            request.UserEmail,
            request.GuestName);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("leave")]
    public async Task<IActionResult> LeaveMeeting([FromBody] LeaveMeetingRequest request)
    {
        var result = await _meetingService.LeaveMeetingAsync(request.JoinToken);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet("{roomCode}/participants")]
    public async Task<IActionResult> GetParticipants(string roomCode)
    {
        var result = await _meetingService.GetParticipantsAsync(roomCode);
        return Ok(result);
    }

    [HttpGet("participant/{joinToken}")]
    public async Task<IActionResult> GetParticipantByToken(string joinToken)
    {
        var result = await _meetingService.GetParticipantByTokenAsync(joinToken);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{roomCode}/invite")]
    public async Task<IActionResult> Invite(string roomCode, [FromBody] InviteRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        if (request.InviteeEmails == null || !request.InviteeEmails.Any())
            return BadRequest("Danh sách email rỗng");

        var result = await _meetingService.InviteAsync(roomCode, email, request.InviteeEmails);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("invite/{inviteId}/respond")]
    public async Task<IActionResult> RespondInvite(int inviteId, [FromBody] InviteRespondRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var result = await _meetingService.RespondInviteAsync(inviteId, email, request.Status);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("invited")]
    public async Task<IActionResult> GetInvitedMeetings()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var result = await _meetingService.GetAcceptedInviteMeetingsAsync(email);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }
}

