namespace Tournament.Common.DTOs
{
    public class TournamentParticipationRuleDto
    {
        public decimal MinBetAmount { get; set; }
        public decimal? MinOdd { get; set; }
        public int? MinSpinsCount { get; set; }
        public int? MinEvents { get; set; }
        public string GameCode { get; set; }
    }
}