using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Tournament.Domain.DataBase.DBContext;
using Tournament.Domain.DataBase.Entity;
using Tournament.Workerservices.IWokerRepo;

namespace Tournament.Workerservices.WorkerRepo
{
    public class EventRepo : IEventRepo
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public EventRepo(IDbContextFactory<ApplicationDbContext> context)
        {
            _contextFactory = context;
        }
        public async Task AddTournamenntEvent(TournamentEvent entity, long tournamentId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var tournament = await context.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);
            if (entity.Action == Common.Enums.Action.BetWin || entity.Action == Common.Enums.Action.SpinWin)
            {
                foreach (var item in entity.Points)
                {
                    item.TournamentId = tournament.Id;
                    item.Tournament = tournament;
                }
            }
            try
            {
                entity.EntityRefId = tournament.Id;
                entity.Tournament = tournament;
                await context.TournamentEvents.AddAsync(entity);//date added need to change or not not confirmed?
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public async Task<TournamentEntity> GetByIdAsync(long id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Tournaments.Include(_ => _.ParticipationRules).FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TournamentParticipant> GetParticipantByIdAsync(long id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TournamentParticipants.FirstOrDefaultAsync(p => p.AccountId == id);
        }

        public async Task<List<TournamentEvent>> GetTournamentEventByAccIdAndTicketId(string id, long ticketId, long tournamentId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TournamentEvents.Where(_ => _.AccountId.ToString() == id && _.TicketId == ticketId && _.Tournament.Id == tournamentId).ToListAsync();
        }

        public async Task<LeaderboardEntry> GetLeaderboardEntry(long accountId, long tournamentId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.LeaderboardEntries.FirstOrDefaultAsync(_ => _.AccountId == accountId && _.TournamentId == tournamentId);
        }

        public async Task AddLeaderboardEntry(LeaderboardEntry entry)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var tournament = await context.Tournaments.FirstOrDefaultAsync(t => t.Id == entry.TournamentId);
                var participant = await context.TournamentParticipants.FirstOrDefaultAsync(p => p.AccountId == entry.AccountId && p.TournamentId == tournament.Id);
                entry.Tournament = tournament;
                entry.TournamentPlayer = participant;
                await context.LeaderboardEntries.AddAsync(entry);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateLeaderboardEntry(LeaderboardEntry entry)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.LeaderboardEntries.Update(entry);
            await context.SaveChangesAsync();
        }

        public async Task<TournamentParticipant> GetParticipantByIdAsync(long id, long tournamentId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TournamentParticipants.FirstOrDefaultAsync(p => p.AccountId == id && p.TournamentId == tournamentId);
        }
    }
}
