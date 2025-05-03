
namespace Tournament.Domain.DataBase.Entity;

public class TournamentEvent
{
    public long Id { get; set; }
    public Guid EventRefId { get; set; }
    public long TournamentId { get; set; }
    public long AccountId { get; set; }
    public int EntityType { get; set; }
    public int ActionType { get; set; }
    public decimal StakeAmount { get; set; }
    public decimal? WinAmount { get; set; }
    public decimal? Odd { get; set; }
    public string MetaData { get; set; }
    public bool IsQualifying { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public TournamentEntity Tournament { get; set; }
    public ICollection<TournamentPoint> Points { get; set; }
}
