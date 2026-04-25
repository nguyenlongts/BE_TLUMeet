public class MeetingRepository : IMeetingRepository
{
    private readonly MeetingDbContext _context;

    public MeetingRepository(MeetingDbContext context)
    {
        _context = context;
    }

    public async Task<Meeting?> GetByIdAsync(int id)
        => await _context.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Meeting?> GetByRoomCodeAsync(string roomCode)
       => await _context.Meetings
           .Include(m => m.Participants)
               .ThenInclude(p => p.Role)
           .FirstOrDefaultAsync(m => m.RoomCode == roomCode);


    public async Task<List<Meeting>> GetAllAsync()
        => await _context.Meetings.ToListAsync();

    public async Task<List<Meeting>> GetByHostEmailAsync(string hostEmail)
        => await _context.Meetings
            .Where(m => m.HostEmail == hostEmail)
            .ToListAsync();

    public async Task<Meeting> CreateAsync(Meeting meeting)
    {

        _context.Meetings.Add(meeting);
        return meeting;
    }

    public async Task UpdateAsync(Meeting meeting)
    {
        _context.Meetings.Update(meeting);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting == null)
            return false;

        _context.Meetings.Remove(meeting);
        return true;
    }

    public async Task<List<MeetingParticipant>> GetParticipantsByRoomCodeAsync(string roomCode)
        => await _context.Participants
            .Include(p => p.Meeting).Include(p=>p.Role)
            .Where(p => p.Meeting.RoomCode == roomCode)
            .ToListAsync();

    public async Task<MeetingParticipant?> GetParticipantByIdAsync(int id)
     => await _context.Participants
         .Include(p => p.Role)
         .FirstOrDefaultAsync(p => p.Id == id);


    public async Task<MeetingParticipant?> GetParticipantByTokenAsync(string joinToken)
        => await _context.Participants
            .Include(p => p.Role)
            .FirstOrDefaultAsync(p => p.JoinToken == joinToken);


    public async Task AddParticipantAsync(MeetingParticipant participant)
    {
        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateParticipantAsync(MeetingParticipant participant)
    {
        _context.Participants.Update(participant);
        await _context.SaveChangesAsync();
    }

    public async Task AddGuestAsync(Guest guest)
    {
        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();
    }

    public async Task AddInviteAsync(MeetingInvite invite)
    {
        _context.Invites.Add(invite);
    }

    public async Task<MeetingInvite?> GetInviteByIdAsync(int id)
        => await _context.Invites.FirstOrDefaultAsync(i => i.Id == id);

    public async Task UpdateInviteAsync(MeetingInvite invite)
    {
        _context.Invites.Update(invite);
    }
}
