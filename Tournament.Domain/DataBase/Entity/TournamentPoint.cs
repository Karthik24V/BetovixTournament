
namespace Tournament.Domain.DataBase.Entity;

public class TournamentPoint
{
    public long Id { get; set; }
    public long TournamentId { get; set; }
    public long AccountId { get; set; }
    public Guid EventRefId { get; set; }
    public long EventId { get; set; }
    public decimal Points { get; set; }
    public decimal WinAmount { get; set; }
    public DateTime CreatedOn { get; set; }
    public TournamentEntity Tournament { get; set; }
    public TournamentEvent Event { get; set; }
}
