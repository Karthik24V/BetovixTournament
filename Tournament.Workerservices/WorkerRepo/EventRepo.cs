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
            var tournament =await context.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);
            if (entity.Action == Common.Enums.Action.BetWin || entity.Action == Common.Enums.Action.SpinWin) {
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
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
        }

        public async Task<TournamentEntity> GetByIdAsync(long id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Tournaments.Include(_ => _.ParticipationRules).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TournamentParticipant> GetParticipantByIdAsync(long id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TournamentParticipants.FirstOrDefaultAsync(p => p.AccountId == id);
        }

        public async Task<List<TournamentEvent>> GetTournamentEventByAccIdAndEventRefId(string id, Guid eventId, long tournamentId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TournamentEvents.Where(_ => _.AccountId.ToString() == id && _.EventRefId == eventId && _.EntityRefId == tournamentId).ToListAsync();
        }
    }
}
