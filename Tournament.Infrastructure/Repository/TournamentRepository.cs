using Tournament.Domain.DataBase.Entity;
using Tournament.Data.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using Tournament.Domain.DataBase.DBContext;
using Tournament.Common.DTOs;
using Microsoft.Extensions.Logging;

namespace Tournament.Data.Repository
{
    /// <summary>
    /// Repository for tournament-related data access operations.
    /// </summary>
    public class TournamentRepository : ITournamentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TournamentRepository> _logger;

        /// <summary>
        /// Constructor for TournamentRepository.
        /// </summary>
        public TournamentRepository(ApplicationDbContext context, ILogger<TournamentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new tournament entity to the database.
        /// </summary>
        public async Task AddAsync(TournamentEntity entity)
        {
            _logger.LogInformation("Adding new tournament: {Name}", entity.Name);
            _context.Tournaments.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Tournament added with Id: {Id}", entity.Id);
        }

        /// <summary>
        /// Retrieves a tournament with its participation rules by Id.
        /// </summary>
        public async Task<TournamentEntity> GetTournamentWithRulesByIdAsync(long id)
        {
            try
            {
                _logger.LogInformation("Fetching tournament with rules. Id: {Id}", id);
                var result = await _context.Tournaments
                    .Include(t => t.ParticipationRules)
                    .FirstOrDefaultAsync(t => t.Id == id);
                if (result == null)
                    _logger.LogWarning("Tournament not found. Id: {Id}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tournament with rules. Id: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing tournament entity in the database.
        /// </summary>
        public async Task UpdateAsync(TournamentEntity entity)
        {
            _logger.LogInformation("Updating tournament. Id: {Id}", entity.Id);
            _context.Tournaments.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Tournament updated. Id: {Id}", entity.Id);
        }

        /// <summary>
        /// Retrieves a tournament by Id, including its participants.
        /// </summary>
        public async Task<TournamentEntity> GetByIdAsync(long id)
        {
            _logger.LogInformation("Fetching tournament by Id: {Id}", id);
            var result = await _context.Tournaments.Include(_ => _.Participants).FirstOrDefaultAsync(t => t.Id == id);
            if (result == null)
                _logger.LogWarning("Tournament not found. Id: {Id}", id);
            return result;
        }

        /// <summary>
        /// Adds a new participant to a tournament.
        /// </summary>
        public async Task AddParticipant(TournamentParticipant entity)
        {
            _logger.LogInformation("Adding participant. TournamentId: {TournamentId}, AccountId: {AccountId}", entity.TournamentId, entity.AccountId);
            _context.TournamentParticipants.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Participant added. Id: {Id}", entity.Id);
        }

        /// <summary>
        /// Retrieves a user by Id.
        /// </summary>
        public async Task<User> GetUserByIdAsync(long id)
        {
            _logger.LogInformation("Fetching user by Id: {Id}", id);
            var result = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
            if (result == null)
                _logger.LogWarning("User not found. Id: {Id}", id);
            return result;
        }

        /// <summary>
        /// Retrieves a participant by account Id and tournament Id.
        /// </summary>
        public async Task<TournamentParticipant> GetParticipantByIdAsync(long id, long tournamentId)
        {
            _logger.LogInformation("Fetching participant. AccountId: {AccountId}, TournamentId: {TournamentId}", id, tournamentId);
            var result = await _context.TournamentParticipants.FirstOrDefaultAsync(p => p.AccountId == id && p.TournamentId == tournamentId);
            if (result == null)
                _logger.LogWarning("Participant not found. AccountId: {AccountId}, TournamentId: {TournamentId}", id, tournamentId);
            return result;
        }

        /// <summary>
        /// Retrieves all tournament points for a given tournament.
        /// </summary>
        public async Task<ICollection<TournamentPoint>> GetTournamentPointsByIdAsync(long tournamentId)
        {
            _logger.LogInformation("Fetching tournament points. TournamentId: {TournamentId}", tournamentId);
            var result = await _context.TournamentPoints.Where(_ => _.TournamentId == tournamentId).ToListAsync();
            _logger.LogInformation("Fetched {Count} tournament points for TournamentId: {TournamentId}", result.Count, tournamentId);
            return result;
        }

        /// <summary>
        /// Retrieves all tournaments.
        /// </summary>
        public async Task<IQueryable<TournamentEntity>> GetAllTournament()
        {
            _logger.LogInformation("Fetching all tournaments.");
            var result = _context.Tournaments.AsQueryable();
            _logger.LogInformation("Fetched {Count} tournaments.", result.ToList().Count);
            return result;
        }
        /// <summary>
        /// Retrieves all leaderboard entries for a given tournament.
        /// </summary>
        public async Task<IQueryable<LeaderboardEntry>> GetLeaderboardEntriesByTournamentIdAsync(long tournamentId)
        {
            _logger.LogInformation("Fetching leaderboard entries. TournamentId: {TournamentId}", tournamentId);
            var result = _context.LeaderboardEntries
                .Where(le => le.TournamentId == tournamentId)
                .AsQueryable();
            //_logger.LogInformation("Fetched {Count} leaderboard entries for TournamentId: {TournamentId}", result.Count, tournamentId.ToString());
            return result;
        }

        /// <summary>
        /// Fetches the winners of the minimum 3 most recently completed tournaments, sorted by best multiplier.
        /// </summary>
        public async Task<IList<(TournamentEntity Tournament, LeaderboardEntry Winner)>> GetRecentTournamentWinnersAsync(int minCount = 3)
        {
            _logger.LogInformation("Fetching winners of the {MinCount} most recently completed tournaments.", minCount);

            // Step 1: Get the most recent completed tournaments (where EndDate < now), min 3
            var recentTournaments = await _context.Tournaments
                .Where(t => t.EndDate < DateTime.UtcNow && !t.IsDeleted && !t.IsActive)
                .OrderByDescending(t => t.EndDate)
                .Take(minCount)
                .ToListAsync();

            var winners = new List<(TournamentEntity, LeaderboardEntry)>();

            foreach (var tournament in recentTournaments)
            {
                // Step 2: For each tournament, get the leaderboard entry with the best multiplier
                var winner = await _context.LeaderboardEntries
                    .Where(le => le.TournamentId == tournament.Id)
                    .OrderByDescending(le => le.BestMultiplier)
                    .FirstOrDefaultAsync();

                if (winner != null)
                {
                    winners.Add((tournament, winner));
                }
            }

            _logger.LogInformation("Fetched winners for {Count} tournaments.", winners.Count);
            return winners;
        }

    }
}