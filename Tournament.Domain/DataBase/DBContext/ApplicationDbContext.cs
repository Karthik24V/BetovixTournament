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

}