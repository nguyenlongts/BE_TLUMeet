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

    // Cuộc họp sắp diễn ra (trong khoảng [nowUtc, windowEndUtc]) chưa gửi nhắc lịch
    Task<List<Meeting>> GetDueForReminderAsync(DateTime nowUtc, DateTime windowEndUtc);


}