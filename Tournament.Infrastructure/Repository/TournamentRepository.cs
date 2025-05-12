using Tournament.Domain.DataBase.Entity;
using Tournament.Data.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using Tournament.Domain.DataBase.DBContext;

namespace Tournament.Data.Repository
{
    public class TournamentRepository : ITournamentRepository
    {
        private readonly ApplicationDbContext _context;

        public TournamentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TournamentEntity entity)
        {
            _context.Tournaments.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<TournamentEntity> GetTournamentWithRulesByIdAsync(long id)
        {
            return await _context.Tournaments
                .Include(t => t.ParticipationRules)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(TournamentEntity entity)
        {
            _context.Tournaments.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<TournamentEntity> GetByIdAsync(long id)
        {
            return await _context.Tournaments.FirstOrDefaultAsync(t => t.Id == id);
        }

    }
}