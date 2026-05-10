using MeetingService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingService.Domain.Interfaces
{
    public interface IInviteRepository
    {
        Task AddAsync(MeetingInvite invite);
        Task<MeetingInvite?> GetByIdAsync(int id);
        Task UpdateAsync(MeetingInvite invite);

        Task<List<MeetingInvite>> GetAcceptedByMeetingIdAsync(int meetingId);
        Task<List<MeetingInvite>> GetAcceptedByEmailAsync(string email);
        Task<bool> ExistsPendingAsync(int meetingId, string email);
    }
}
