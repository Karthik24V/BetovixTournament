using Microsoft.EntityFrameworkCore;
using Tournament.Common.Enums;
using Tournament.Domain.DataBase.Entity;

namespace Tournament.Domain.DataBase.DBContext;

public class ApplicationDbContext : DbContext
{
    public DbSet<TournamentEntity> Tournaments { get; set; }
    public DbSet<TournamentParticipationRule> TournamentParticipationRules { get; set; }
    public DbSet<TournamentParticipant> TournamentParticipants { get; set; }
    public DbSet<TournamentEvent> TournamentEvents { get; set; }
    public DbSet<TournamentPoint> TournamentPoints { get; set; }
    public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }
    public DbSet<TournamentPrize> TournamentPrizes { get; set; }
    public DbSet<User> User { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tournament
        modelBuilder.Entity<TournamentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasMany(e => e.ParticipationRules)
                  .WithOne(r => r.Tournament)
                  .HasForeignKey(r => r.TournamentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Participants)
                  .WithOne(p => p.Tournament)
                  .HasForeignKey(p => p.TournamentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Events)
                  .WithOne(ev => ev.Tournament)
                  .HasForeignKey(ev => ev.TournamentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.LeaderboardEntries)
                  .WithOne(l => l.Tournament)
                  .HasForeignKey(l => l.TournamentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Prizes)
                  .WithOne(pr => pr.Tournament)
                  .HasForeignKey(pr => pr.TournamentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Points)
                  .WithOne(pt => pt.Tournament)
                  .HasForeignKey(pt => pt.TournamentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Participation Rule
        modelBuilder.Entity<TournamentParticipationRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameCode).HasMaxLength(100);
        });

        // Participant
        modelBuilder.Entity<TournamentParticipant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(p => p.User).WithMany(u => u.TournamentParticipations).HasForeignKey(p => p.UserId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UniqueName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(300);
        });


        // TournamentEvent
        modelBuilder.Entity<TournamentEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetaData).IsRequired();
            entity.Property(e => e.IsQualifying).HasDefaultValue(false);
        });

        // TournamentPoint
        modelBuilder.Entity<TournamentPoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(200);
        });

        // LeaderboardEntry
        modelBuilder.Entity<LeaderboardEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TournamentId, e.AccountId }).IsUnique();

            entity.HasOne(e => e.TournamentPlayer)
                  .WithMany()
                  .HasForeignKey(e => e.ParticipantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Rank)
            .HasConversion(e => e.ToString(), e => (Rank)Enum.Parse(typeof(Rank), e))
            .HasColumnType("text");

        });

        // TournamentPrize
        modelBuilder.Entity<TournamentPrize>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        base.OnModelCreating(modelBuilder);
    }
}