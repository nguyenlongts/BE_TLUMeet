using AuthService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message);
    }
}
