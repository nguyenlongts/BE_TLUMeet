public class InviteRepository : IInviteRepository
{
    private readonly MeetingDbContext _context;

    public InviteRepository(MeetingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MeetingInvite invite)
    {
        _context.Invites.Add(invite);
    }

    public async Task<MeetingInvite?> GetByIdAsync(int id)
    {
       return await _context.Invites.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task UpdateInviteAsync(MeetingInvite invite)
    {
        _context.Invites.Update(invite);
    }
    public async Task<List<MeetingInvite>> GetAcceptedByMeetingIdAsync(int meetingId)
    {
        return await _context.Invites.Where(i => i.MeetingId == meetingId && i.Status == "Accepted").ToListAsync();
    }
    public async Task<List<MeetingInvite>> GetAcceptedByEmailAsync(string email)
    {
        return await _context.Invites.Include(i => i.Meeting).Where(i => i.InviteeEmail == email && i.Status == "Accepted").ToListAsync();
    }

    public async Task UpdateAsync(MeetingInvite invite)
    {
         _context.Invites.Update(invite);
    }

    public async Task<bool> ExistsPendingAsync(int meetingId, string email)
    {
        return await _context.Invites.AnyAsync(i =>
            i.MeetingId == meetingId &&
            i.InviteeEmail == email &&
            i.Status == "Pending");
    }
}