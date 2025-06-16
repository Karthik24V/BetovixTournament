using Tournament.Common.DTOs;
using Tournament.Business.IServices;
using Tournament.Data.IRepository;
using Tournament.Domain.DataBase.Entity;
using Tournament.Data.Repository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Tournament.Business.Services
{
    /// <summary>
    /// Service for managing tournaments and related operations.
    /// </summary>
    public class TournamentService : ITournamentService
    {
        private readonly ITournamentRepository _tournamentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TournamentService> _logger; // Logger field

        /// <summary>
        /// Constructor for TournamentService.
        /// </summary>
        public TournamentService(ITournamentRepository repository, IMapper mapper, ILogger<TournamentService> logger)
        {
            _tournamentRepository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new tournament with participation rules.
        /// </summary>
        public async Task<TournamentDto> CreateTournamentAsync(CreateTournamentDto dto)
        {
            _logger.LogInformation("Creating tournament: {Name}", dto.Name);

            var entity = _mapper.Map<TournamentEntity>(dto);

            // Map ParticipationRule (one-to-one)
            var rule = _mapper.Map<TournamentParticipationRule>(dto.ParticipationRule);
            rule.Tournament = entity;
            entity.ParticipationRules = rule;

            entity.CreatedBy = "system"; // Replace with actual user if available
            entity.CreatedOn = DateTime.UtcNow;

            await _tournamentRepository.AddAsync(entity);

            _logger.LogInformation("Tournament created with Id: {Id}", entity.Id);

            return _mapper.Map<TournamentDto>(entity);
        }

        /// <summary>
        /// Retrieves a tournament by its Id, including participation rules.
        /// </summary>
        public async Task<TournamentDto> GetTournamentByIdAsync(long id)
        {
            _logger.LogInformation("Fetching tournament by Id: {Id}", id);

            var entity = await _tournamentRepository.GetTournamentWithRulesByIdAsync(id);
            if (entity == null || entity.IsDeleted)
            {
                _logger.LogWarning("Tournament not found or deleted. Id: {Id}", id);
                return null;
            }

            _logger.LogInformation("Tournament found. Id: {Id}", id);
            return _mapper.Map<TournamentDto>(entity);
        }

        /// <summary>
        /// Updates an existing tournament and its participation rules.
        /// </summary>
        public async Task<TournamentDto> UpdateTournamentAsync(long id, UpdateTournamentDto dto)
        {
            _logger.LogInformation("Updating tournament. Id: {Id}", id);

            var entity = await _tournamentRepository.GetTournamentWithRulesByIdAsync(id);
            if (entity == null || entity.IsDeleted)
            {
                _logger.LogWarning("Tournament not found or deleted for update. Id: {Id}", id);
                return null;
            }

            // Map updatable fields
            _mapper.Map(dto, entity); // handles UpdatedOn via mapping profile

            // Update Rule (manual since it's a nested object)
            if (dto.Rules != null)
            {
                if (entity.ParticipationRules == null)
                    entity.ParticipationRules = new TournamentParticipationRule { TournamentId = id };

                _mapper.Map(dto.Rules, entity.ParticipationRules);
            }

            await _tournamentRepository.UpdateAsync(entity);

            _logger.LogInformation("Tournament updated successfully. Id: {Id}", id);

            return _mapper.Map<TournamentDto>(entity);
        }

        /// <summary>
        /// Soft deletes a tournament by marking it as deleted.
        /// </summary>
        public async Task<bool> DeleteTournamentAsync(long id)
        {
            _logger.LogInformation("Deleting tournament. Id: {Id}", id);

            var entity = await _tournamentRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
            {
                _logger.LogWarning("Tournament not found or already deleted. Id: {Id}", id);
                return false;
            }

            entity.IsDeleted = true;
            entity.UpdatedOn = DateTime.UtcNow;

            await _tournamentRepository.UpdateAsync(entity);

            _logger.LogInformation("Tournament marked as deleted. Id: {Id}", id);

            return true;
        }

        /// <summary>
        /// Adds a participant to a tournament if eligible and not already joined.
        /// </summary>
        public async Task<bool> JoinTournamentAsync(ParticipationDto dto)
        {
            _logger.LogInformation("Joining tournament. TournamentId: {TournamentId}, AccountId: {AccountId}", dto.TournamentId, dto.AccountId);

            try
            {
                var tournament = await _tournamentRepository.GetByIdAsync(dto.TournamentId);
                var participant = new TournamentParticipant()
                {
                    TournamentId = dto.TournamentId,
                    AccountId = dto.AccountId,
                    UserId = dto.UserId
                };

                // Check if the current time is within the tournament period
                var isValidEntry = DateTime.UtcNow >= tournament.CreatedOn && DateTime.UtcNow <= tournament.EndDate ? true : false;
                // Ensure the participant is not already registered
                var IsExistingUser = tournament.Participants.FirstOrDefault(_ => _.AccountId == participant.AccountId && _.TournamentId == participant.TournamentId) == null;

                if (isValidEntry && IsExistingUser)
                {
                    participant.Tournament = tournament;
                    var user = await _tournamentRepository.GetUserByIdAsync(participant.UserId); // For now User part in pending.
                    if (user != null)
                    {
                        participant.User = user;
                    }
                    // Placeholder user creation (should be replaced with actual user logic)
                    participant.User = new User()
                    {
                        RealName = "",
                        Country = "",
                        UniqueName = "",
                        Email = "",
                        IsActive = true,
                        Points = 0,
                        RegisteredOn = DateTime.UtcNow,
                    };
                    participant.JoinedOn = DateTime.UtcNow;
                    participant.EligibilityPassed = true;
                    await _tournamentRepository.AddParticipant(participant);
                    tournament.Participants.Add(await _tournamentRepository.GetParticipantByIdAsync(participant.AccountId, participant.TournamentId));
                    await _tournamentRepository.UpdateAsync(tournament);

                    _logger.LogInformation("Participant joined successfully. TournamentId: {TournamentId}, AccountId: {AccountId}", dto.TournamentId, dto.AccountId);

                    return true;
                }

                _logger.LogWarning("Participant not eligible or already joined. TournamentId: {TournamentId}, AccountId: {AccountId}", dto.TournamentId, dto.AccountId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining tournament. TournamentId: {TournamentId}, AccountId: {AccountId}", dto.TournamentId, dto.AccountId);
                return false;
            }
        }

        /// <summary>
        /// Gets the participation status of a user in a tournament.
        /// </summary>
        public async Task<ParticipationStatusDto> ParticipantStatustAsync(long id, long TournamentId)
        {
            _logger.LogInformation("Checking participant status. ParticipantId: {ParticipantId}, TournamentId: {TournamentId}", id, TournamentId);

            try
            {
                var participant = await _tournamentRepository.GetParticipantByIdAsync(id, TournamentId);

                if (participant != null)
                {
                    _logger.LogInformation("Participant found. ParticipantId: {ParticipantId}, TournamentId: {TournamentId}", id, TournamentId);

                    return new ParticipationStatusDto
                    {
                        Joined = true,
                        JoinedOn = participant.JoinedOn,
                        Eligible = participant.EligibilityPassed,
                    };
                }

                _logger.LogWarning("Participant not found. ParticipantId: {ParticipantId}, TournamentId: {TournamentId}", id, TournamentId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking participant status. ParticipantId: {ParticipantId}, TournamentId: {TournamentId}", id, TournamentId);
                return null;
            }
        }

        /// <summary>
        /// Retrieves leaderboard data for a tournament, with paging and sorting.
        /// </summary>
        public async Task<PagedResult<LeaderboardDto>> GetLeaderBoardData(long tournamentId, int page, int pageSize, string sortBy)
        {
            try
            {
                _logger.LogInformation("Fetching leaderboard data. TournamentId: {TournamentId}, Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}", tournamentId, page, pageSize, sortBy);

                var leaderBoardEntries = await _tournamentRepository.GetLeaderboardEntriesByTournamentIdAsync(tournamentId);

                // Sort
                var sorted = sortBy == "multiplier"
                    ? leaderBoardEntries.OrderByDescending(x => x.BestMultiplier)
                    : leaderBoardEntries.OrderByDescending(x => x.TotalPoints);

                // Total count for pagination
                var totalCount = sorted.Count();

                // Paging - executed in DB
                var pagedList = sorted
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList(); // Fetch from DB before using index

                // Projection with rank
                var items = pagedList
                    .Select((x, index) => new LeaderboardDto
                    {
                        Rank = (page - 1) * pageSize + index + 1,
                        AccountId = x.AccountId,
                        TotalPoints = (int)x.TotalPoints,
                        BestMultiplier = x.BestMultiplier
                    })
                    .ToList();

                _logger.LogInformation("Leaderboard data fetched. TournamentId: {TournamentId}, Count: {Count}", tournamentId, items.Count);

                return new PagedResult<LeaderboardDto>
                {
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching leaderboard data for TournamentId: {TournamentId}", tournamentId);
                throw;
            }
        }

        /// <summary>
        /// Gets all current tournaments (active and ongoing).
        /// </summary>
        public async Task<ICollection<TournamentDto>> GetCurrentTournament()
        {
            _logger.LogInformation("Fetching current tournaments.");

            var tournaments = await _tournamentRepository.GetAllTournament();

            // Filter tournaments that are currently active and not deleted
            var Result = tournaments.Where(_ => _.StartDate <= DateTime.Now && _.EndDate >= DateTime.Now && _.IsActive && !_.IsDeleted)
                               .Select(_ => new TournamentDto
                               {
                                   Id = _.Id,
                                   Name = _.Name,
                                   GameType = _.GameType,
                                   IsActive = _.IsActive,
                                   StartDate = _.StartDate,
                                   EndDate = _.EndDate,
                                   CreatedBy = _.CreatedBy,
                                   Description = _.Description,
                                   IsDeleted = _.IsDeleted,
                               }).ToList();

            _logger.LogInformation("Current tournaments fetched. Count: {Count}", Result.Count);

            return Result;
        }

        /// <summary>
        /// Gets all upcoming tournaments (active and not started yet).
        /// </summary>
        public async Task<ICollection<TournamentDto>> GetUpcomingTournament()
        {
            _logger.LogInformation("Fetching upcoming tournaments.");

            var tournaments = await _tournamentRepository.GetAllTournament();

            // Filter tournaments that are upcoming, active, and not deleted
            var Result = tournaments.Where(_ => _.StartDate >= DateTime.UtcNow && _.IsActive && !_.IsDeleted)
                                .OrderByDescending(x => x.StartDate)
                               .Select(_ => new TournamentDto
                               {
                                   Id = _.Id,
                                   Name = _.Name,
                                   GameType = _.GameType,
                                   IsActive = _.IsActive,
                                   StartDate = _.StartDate,
                                   EndDate = _.EndDate,
                                   CreatedBy = _.CreatedBy,
                                   Description = _.Description,
                                   IsDeleted = _.IsDeleted,
                               }).ToList();

            _logger.LogInformation("Upcoming tournaments fetched. Count: {Count}", Result.Count);

            return Result;
        }

        /// <summary>
        /// Retrieves a list of recent tournament winners, with a minimum count.
        /// </summary>
        /// <param name="minCount">The minimum number of winners to retrieve.</param>
        /// <returns>A list of TournamentWinnerDto containing recent winners' details.</returns>
        public async Task<IList<TournamentWinnerDto>> GetRecentTournamentWinnersAsync(int minCount = 3)
        {
            _logger.LogInformation("Fetching recent tournament winners. MinCount: {MinCount}", minCount);

            var winners = await _tournamentRepository.GetRecentTournamentWinnersAsync(minCount);

            if (winners == null || winners.Count == 0)
            {
                _logger.LogWarning("No recent tournament winners found. MinCount: {MinCount}", minCount);
                return new List<TournamentWinnerDto>();
            }

            var result = winners.Select(w => new TournamentWinnerDto
            {
                TournamentId = w.Tournament.Id,
                TournamentName = w.Tournament.Name,
                WinnerAccountId = w.Winner.AccountId,
                WinnerBestMultiplier = w.Winner.BestMultiplier
            }).ToList();

            _logger.LogInformation("Recent tournament winners fetched. Count: {Count}", result.Count);

            return result;
        }
    }
}