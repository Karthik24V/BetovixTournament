using Tournament.Domain.DataBase.Entity;

namespace Tournament.Data.IRepository
{
    public interface ITournamentRepository
    {
        Task AddAsync(TournamentEntity entity);
        Task<TournamentEntity> GetTournamentWithRulesByIdAsync(long id);
        Task UpdateAsync(TournamentEntity entity);
        Task<TournamentEntity> GetByIdAsync(long id);
    }
}