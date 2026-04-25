using MeetingService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingService.Domain.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message);
        Task<List<OutboxMessage>> GetPendingMessagesAsync();
    }
}
