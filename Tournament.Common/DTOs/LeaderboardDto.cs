using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament.Common.DTOs
{
    public class LeaderboardDto
    {
        public int Rank { get; set; }
        public long AccountId { get; set; }
        public int TotalPoints { get; set; }
        public decimal BestMultiplier { get; set; }
    }
}
