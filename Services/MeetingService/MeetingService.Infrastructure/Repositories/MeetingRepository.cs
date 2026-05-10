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

}
