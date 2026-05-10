public class ParticipantRepository : IParticipantRepository
{
    private readonly MeetingDbContext _context;

    public ParticipantRepository(MeetingDbContext context)
    {
        _context = context;
    }


    public async Task<List<MeetingParticipant>> GetByRoomCodeAsync(string roomCode)
    {
        return await _context.Participants
            .Include(p => p.Meeting).Include(p => p.Role)
            .Where(p => p.Meeting.RoomCode == roomCode)
            .ToListAsync();
    }

    public async Task<MeetingParticipant?> GetByIdAsync(int id)
    {
        return await _context.Participants
         .Include(p => p.Role)
         .FirstOrDefaultAsync(p => p.Id == id);
    }

public async Task<MeetingParticipant?> GetByTokenAsync(string joinToken)
    {
        return await _context.Participants
            .Include(p => p.Role)
            .FirstOrDefaultAsync(p => p.JoinToken == joinToken);
    }

    public async Task AddAsync(MeetingParticipant participant)
    {
        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MeetingParticipant participant)
    {
        _context.Participants.Update(participant);
        await _context.SaveChangesAsync();
    }

    public async Task AddGuestAsync(Guest guest)
    {
        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();
    }


}
