using Tournament.Domain.DataBase.Entity;
using Tournament.Data.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using Tournament.Domain.DataBase.DBContext;
using Tournament.Common.DTOs;

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
            return await _context.Tournaments.Include(_ => _.Participants).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddParticipant(TournamentParticipant entity)
        {
            _context.TournamentParticipants.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByIdAsync(long id)
        {
           return await _context.User.FirstOrDefaultAsync(u => u.Id == id);

        }

        public async Task<TournamentParticipant> GetParticipantByIdAsync(long id, long tournamentId)
        {
           return await _context.TournamentParticipants.FirstOrDefaultAsync( p => p.AccountId == id && p.TournamentId == tournamentId);
        }

        public async Task<ICollection<TournamentPoint>> GetTournamentPointsByIdAsync(long tournamentId)
        {
            return await _context.TournamentPoints.Where(_ => _.TournamentId == tournamentId).ToListAsync();
        }

        public async Task<ICollection<TournamentEntity>> GetAllTournament()
        {
            return await _context.Tournaments.ToListAsync();
        }
    }
}