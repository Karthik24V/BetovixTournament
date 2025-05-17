using Tournament.Common.DTOs;
using Tournament.Domain.DataBase.Entity;

namespace Tournament.Data.IRepository
{
    public interface ITournamentRepository
    {
        Task AddAsync(TournamentEntity entity);
        Task<TournamentEntity> GetTournamentWithRulesByIdAsync(long id);
        Task UpdateAsync(TournamentEntity entity);
        Task<TournamentEntity> GetByIdAsync(long id);
        Task<User> GetUserByIdAsync(long id);
        Task<TournamentParticipant> GetParticipantByIdAsync(long id, long TournamentId);
        Task AddParticipant(TournamentParticipant entity);
    }
}