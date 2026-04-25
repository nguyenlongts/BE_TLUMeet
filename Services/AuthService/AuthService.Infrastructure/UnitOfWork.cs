using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AuthDbContext _context;
        private IDbContextTransaction? _transaction;

        public IUserRepository Users { get; }
        public IRefreshTokenRepository RefreshTokens { get; }
        public IOutboxRepository OutboxMessages { get; }
        public UnitOfWork(AuthDbContext context, IUserRepository users, IRefreshTokenRepository refreshTokens, IOutboxRepository outbox)
        {
            _context = context;
            Users = users;
            RefreshTokens = refreshTokens;
            OutboxMessages = outbox;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction= await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null) return;
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
             _transaction = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            await _context.DisposeAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null) return;
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
