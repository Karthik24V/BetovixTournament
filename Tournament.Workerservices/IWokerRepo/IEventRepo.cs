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
        Task<List<TournamentEvent>> GetTournamentEventByAccIdAndTicketId(string id, long ticketId, long tournamentId);
        Task<LeaderboardEntry> GetLeaderboardEntry(long accountId, long tournamentId);
        Task AddLeaderboardEntry(LeaderboardEntry entry);
        Task UpdateLeaderboardEntry(LeaderboardEntry entry);
        Task<TournamentParticipant> GetParticipantByIdAsync(long id, long tournamentId);
    }
}
