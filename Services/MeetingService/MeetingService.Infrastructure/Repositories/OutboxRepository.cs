using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingService.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly MeetingDbContext _context;
        public OutboxRepository(MeetingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message);
        }
        public async Task<List<OutboxMessage>> GetPendingMessagesAsync()
        {
            return await _context.OutboxMessages.Where(m => m.OccuredAt == null).ToListAsync();
        }

    }
}
