using System.Collections.Generic;

namespace Tournament.Common.DTOs
{
    public class CreateTournamentDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int GameType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TournamentParticipationRuleDto ParticipationRule { get; set; }
    }
}