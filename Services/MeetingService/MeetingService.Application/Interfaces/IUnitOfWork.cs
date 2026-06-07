using MeetingService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingService.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IMeetingRepository Meetings { get; }
        IParticipantRepository Participants { get; }
        IInviteRepository Invites { get; }
        IOutboxRepository Outbox { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task SaveChangesAsync();
    }
}
