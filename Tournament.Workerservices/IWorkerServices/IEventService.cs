using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament.Common.Dto_s;

namespace Tournament.Workerservices.IWorkerServices
{
    public interface IEventService
    {
        Task HandleBetSpinEvent(TournamentEventDto dto);
        Task HandleBetSpinWinEvent(TournamentEventDto dto);
    }
}
