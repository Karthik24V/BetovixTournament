namespace Tournament.Common.DTOs
{
    public class TournamentWinnerDto
    {
        public long TournamentId { get; set; }
        public string TournamentName { get; set; }
        public long WinnerAccountId { get; set; }
        public decimal WinnerBestMultiplier { get; set; }
    }
}