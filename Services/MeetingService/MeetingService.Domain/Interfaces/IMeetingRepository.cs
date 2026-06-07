namespace MeetingService.Domain.Interfaces;

using MeetingService.Domain.Models;

public interface IMeetingRepository
{
    Task<Meeting?> GetByIdAsync(int id);
    Task<Meeting?> GetByRoomCodeAsync(string roomCode);
    Task<List<Meeting>> GetAllAsync();
    Task<List<Meeting>> GetByHostEmailAsync(string hostEmail);
    Task<Meeting> CreateAsync(Meeting meeting);
    Task UpdateAsync(Meeting meeting);
    Task<bool> DeleteAsync(int id);


}