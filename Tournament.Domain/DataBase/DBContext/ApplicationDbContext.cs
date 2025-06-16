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
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TournamentPoint>()
            .HasOne(tp => tp.Tournament)
            .WithMany(t => t.Points)
            .HasForeignKey(tp => tp.TournamentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevents cascade

        modelBuilder.Entity<TournamentPoint>()
            .HasOne(tp => tp.Event)
            .WithMany(te => te.Points)
            .HasForeignKey(tp => tp.EventId)
            .OnDelete(DeleteBehavior.Restrict); // Also prevent cascade

        modelBuilder.Entity<LeaderboardEntry>()
            .HasOne(e => e.Tournament)
            .WithMany(t => t.LeaderboardEntries)
            .HasForeignKey(e => e.TournamentId)
            .OnDelete(DeleteBehavior.Restrict); // ✅ Avoids multiple cascade path
    }

}