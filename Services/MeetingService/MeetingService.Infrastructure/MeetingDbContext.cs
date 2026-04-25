namespace MeetingService.Infrastructure;

public class MeetingDbContext : DbContext
{
    public MeetingDbContext(DbContextOptions<MeetingDbContext> options) : base(options) { }

    public DbSet<Meeting> Meetings { get; set; } = null!;
    public DbSet<MeetingParticipant> Participants { get; set; } = null!;
    public DbSet<Guest> Guests { get; set; } = null!;
    public DbSet<Role> Roles { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<MeetingInvite> Invites { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RoomCode).IsUnique();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.HostEmail).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });
        modelBuilder.Entity<Role>().HasData(new Role
        {
            Id = 1,
            Name = "Admin",
            Description = "admin"
        },new Role
        {
            Id=2,
            Name="Customer",
            Description="customer"
        },new Role
        {
            Id=3,
            Name="Guest",
            Description="guest"
        });
        modelBuilder.Entity<MeetingInvite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InviteeEmail).IsRequired().HasMaxLength(100);
            entity.Property(e => e.InvitedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Meeting)
                  .WithMany()
                  .HasForeignKey(e => e.MeetingId);
        });

    }
}
