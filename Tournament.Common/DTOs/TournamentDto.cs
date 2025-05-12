namespace Tournament.Common.DTOs
{
    public class TournamentDto
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
        public TournamentParticipationRuleDto ParticipationRule { get; set; }
    }
}