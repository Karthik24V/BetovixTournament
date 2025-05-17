using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament.Common.DTOs
{
    public class ParticipationDto
    {
        public long TournamentId { get; set; }
        public long AccountId { get; set; }
        public long UserId { get; set; }
    }
}
