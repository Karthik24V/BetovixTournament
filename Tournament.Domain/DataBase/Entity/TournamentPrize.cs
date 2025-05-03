
namespace Tournament.Domain.DataBase.Entity;

public class TournamentPrize
{
    public long Id { get; set; }
    public long TournamentId { get; set; }
    public int RankFrom { get; set; }
    public int RankTo { get; set; }
    public decimal PrizeAmount { get; set; }
    public int PrizeType { get; set; }
    public TournamentEntity Tournament { get; set; }
}
