using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament.Domain.DataBase.Entity;

namespace Tournament.Workerservices.IWokerRepo
{
    public interface IEventRepo
    {
        Task<TournamentEntity> GetByIdAsync(long id);
        Task<TournamentParticipant> GetParticipantByIdAsync(long id);
        Task AddTournamenntEvent(TournamentEvent entity, long tournamentId);
        Task<List<TournamentEvent>> GetTournamentEventByAccountId(string id);   
    }
}
