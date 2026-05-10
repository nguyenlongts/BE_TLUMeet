using MeetingService.Application.Interfaces;
using MeetingService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly MeetingDbContext _context;
    private IDbContextTransaction? _transaction;
    public IMeetingRepository Meetings { get; }
    public IOutboxRepository Outbox { get; }
    public IParticipantRepository Participants{ get; }
    public IInviteRepository Invites { get; }
    public UnitOfWork(MeetingDbContext context)
    {
        _context = context;
        Meetings = new MeetingRepository(_context);
        Outbox = new OutboxRepository(_context);
        Participants = new ParticipantRepository(_context);
        Invites = new InviteRepository(_context);
    }


    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }

        await _context.DisposeAsync();
    }
}