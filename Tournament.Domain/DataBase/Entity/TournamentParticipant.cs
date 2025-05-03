
namespace Tournament.Domain.DataBase.Entity;

public class TournamentParticipant
{
    public long Id { get; set; }
    public long TournamentId { get; set; }
    public long AccountId { get; set; }
    public long UserId { get; set; }
    public DateTime JoinedOn { get; set; }
    public bool EligibilityPassed { get; set; }
    public TournamentEntity Tournament { get; set; }
    public User User { get; set; }
}
