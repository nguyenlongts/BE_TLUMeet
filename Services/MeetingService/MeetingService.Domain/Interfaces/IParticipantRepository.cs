using MeetingService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingService.Domain.Interfaces
{
    public interface IParticipantRepository
    {
        Task<List<MeetingParticipant>> GetByRoomCodeAsync(string roomCode);
        Task<MeetingParticipant?> GetByIdAsync(int id);
        Task<MeetingParticipant?> GetByTokenAsync(string joinToken);
        Task AddAsync(MeetingParticipant participant);
        Task UpdateAsync(MeetingParticipant participant);
        Task AddGuestAsync(Guest guest);

    }
}
