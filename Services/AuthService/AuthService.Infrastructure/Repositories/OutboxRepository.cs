using AuthService.Domain.Interfaces;
using AuthService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly AuthDbContext _context;

        public OutboxRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message);
        }
    }
}
