namespace Tournament.Domain.DataBase.Entity;

public class TournamentEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int GameType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }

    public ICollection<TournamentParticipationRule> ParticipationRules { get; set; }
    public ICollection<TournamentParticipant> Participants { get; set; }
    public ICollection<TournamentEvent> Events { get; set; }
    public ICollection<LeaderboardEntry> LeaderboardEntries { get; set; }
    public ICollection<TournamentPrize> Prizes { get; set; }
    public ICollection<TournamentPoint> Points { get; set; }
}
