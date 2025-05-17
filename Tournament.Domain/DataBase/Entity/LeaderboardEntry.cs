using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Tournament.Common.Enums;

namespace Tournament.Domain.DataBase.Entity;

public class LeaderboardEntry
{
    public long Id { get; set; }
    public long TournamentId { get; set; }
    public long AccountId { get; set; }
    public long ParticipantId { get; set; }
    public decimal TotalStake { get; set; }
    public decimal TotalWin { get; set; }
    public decimal TotalPoints { get; set; }
    public decimal BestMultiplier { get; set; }
    public Rank Rank { get; set; }
    public DateTime LastUpdated { get; set; }
    public TournamentEntity Tournament { get; set; }
    public TournamentParticipant TournamentPlayer { get; set; }
}
