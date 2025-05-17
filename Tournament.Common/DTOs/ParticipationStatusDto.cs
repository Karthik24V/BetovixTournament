using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament.Common.DTOs
{
    public class ParticipationStatusDto
    {
        public DateTime JoinedOn { get; set; }
        public bool Joined {  get; set; }
        public bool Eligible { get; set; }
    }
}
