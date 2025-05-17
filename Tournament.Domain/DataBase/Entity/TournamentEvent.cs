
using Tournament.Common.Enums;
using Action = Tournament.Common.Enums.Action;

namespace Tournament.Domain.DataBase.Entity;

public class TournamentEvent
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public Guid EventRefId { get; set; }
    public long EntityRefId { get; set; }
    public Action Action { get; set; }
    public decimal SourceValue { get; set; }
    public decimal? WinAmount { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string MetaData { get; set; }
    public TournamentEntity Tournament { get; set; }
    public ICollection<TournamentPoint>? Points { get; set; }
}
