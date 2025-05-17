using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament.Common.Dto_s
{
    public class BetSpinMetadata
    {
        public StakeInfo stakes { get; set; }
        public List<PointDetail> points { get; set; }
        public bool acceptChanges { get; set; }
        public bool IsBonus { get; set; }
        public int providerId { get; set; }
    }
    public class StakeInfo
    {
        public Dictionary<string, decimal> netpoints { get; set; }
        public Dictionary<string, decimal> netsystems { get; set; }
        public Dictionary<string, decimal> points { get; set; }
        public Dictionary<string, decimal> systems { get; set; }
        public decimal total { get; set; }
    }

    public class PointDetail
    {
        public string MatchName { get; set; }
        public long MatchId { get; set; }
        public string MarketName { get; set; }
        public int MarketTypeId { get; set; }
        public string Line { get; set; }
        public string FieldName { get; set; }
        public string FieldId { get; set; }
        public int FieldTypeId { get; set; }
        public decimal Odd { get; set; }
        public bool Active { get; set; }
        public bool Live { get; set; }
        public DateTime DateOfMatch { get; set; }
        public string SportName { get; set; }
        public string CategoryName { get; set; }
        public string TournamentName { get; set; }
        public long TournamentId { get; set; }
        public long CategoryId { get; set; }
        public long SportId { get; set; }
        public object BB { get; set; }
    }
}
