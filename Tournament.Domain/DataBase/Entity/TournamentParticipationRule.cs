
namespace Tournament.Domain.DataBase.Entity;

public class TournamentParticipationRule
{
    public long Id { get; set; }
    public long TournamentId { get; set; }
    public decimal MinBetAmount { get; set; }
    public decimal? MinOdd { get; set; }
    public int? MinSpinsCount { get; set; }
    public int? MinEvents { get; set; }
    public string GameCode { get; set; }
    public TournamentEntity Tournament { get; set; }
}
