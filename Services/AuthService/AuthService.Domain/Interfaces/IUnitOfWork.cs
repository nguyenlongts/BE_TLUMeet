using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IUserRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IOutboxRepository OutboxMessages { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
