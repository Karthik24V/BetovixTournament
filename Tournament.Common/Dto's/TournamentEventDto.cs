using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament.Common.Dto_s
{
    public class TournamentEventDto
    {
        public long Id { get; set; }
        public long AccountId { get; set; }   
        public Guid EventRefId { get; set; }      
        public string EntityRefId { get; set; }       
        public string Action { get; set; }              
        public decimal SourceValue { get; set; }         
        public string UnitsOfMeasure { get; set; }       
        public bool Send { get; set; }              
        public string MetaData { get; set; }            
        public DateTime DateAdded { get; set; }       
        public DateTime? DateSend { get; set; }
    }
}
