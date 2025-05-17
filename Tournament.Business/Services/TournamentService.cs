using Tournament.Common.DTOs;
using Tournament.Business.IServices;
using Tournament.Data.IRepository;
using Tournament.Domain.DataBase.Entity;
using Tournament.Data.Repository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Tournament.Business.Services
{
    public class TournamentService : ITournamentService
    {
        private readonly ITournamentRepository _tournamentRepository;
        private readonly IMapper _mapper;

        public TournamentService(ITournamentRepository repository, IMapper mapper)
        {
            _tournamentRepository = repository;
            _mapper = mapper;
        }

        public async Task<TournamentDto> CreateTournamentAsync(CreateTournamentDto dto)
        {
            var entity = _mapper.Map<TournamentEntity>(dto);

            // Map ParticipationRule (one-to-one)
            var rule = _mapper.Map<TournamentParticipationRule>(dto.ParticipationRule);
            rule.Tournament = entity;
            entity.ParticipationRules = rule;

            entity.CreatedBy = "system"; // Replace with actual user if available
            entity.CreatedOn = DateTime.UtcNow;

            await _tournamentRepository.AddAsync(entity);
            return _mapper.Map<TournamentDto>(entity);
        }


        public async Task<TournamentDto> GetTournamentByIdAsync(long id)
        {
            var entity = await _tournamentRepository.GetTournamentWithRulesByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return null;

            return _mapper.Map<TournamentDto>(entity);
        }

        public async Task<TournamentDto> UpdateTournamentAsync(long id, UpdateTournamentDto dto)
        {
            var entity = await _tournamentRepository.GetTournamentWithRulesByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return null;

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
            return _mapper.Map<TournamentDto>(entity);
        }

        public async Task<bool> DeleteTournamentAsync(long id)
        {
            var entity = await _tournamentRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return false;

            entity.IsDeleted = true;
            entity.UpdatedOn = DateTime.UtcNow;

            await _tournamentRepository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> JoinTournamentAsync(ParticipationDto dto)   
        {
            try
            {
                var tournament = await _tournamentRepository.GetByIdAsync(dto.TournamentId);
                var participant = new TournamentParticipant()
                {
                    TournamentId = dto.TournamentId,
                    AccountId = dto.AccountId,
                    UserId = dto.UserId
                };

                var isValidEntry = DateTime.UtcNow >= tournament.CreatedOn && DateTime.UtcNow <= tournament.EndDate ? true : false;
                var IsExistingUser = tournament.Participants.FirstOrDefault(_ => _.AccountId == participant.AccountId && _.TournamentId == participant.TournamentId) == null;

                if (isValidEntry && IsExistingUser) {
                    participant.Tournament = tournament;
                    var user = await _tournamentRepository.GetUserByIdAsync(participant.UserId); // For now User part in pending.
                    if (user != null)
                    {
                        participant.User = user;
                    }
                    participant.User = new User()
                    {
                        RealName = "",
                        Country = "",
                        UniqueName = "",
                        Email = "",
                        IsActive= true,
                        Points=0,
                        RegisteredOn = DateTime.UtcNow,
                    };
                    participant.JoinedOn = DateTime.UtcNow;
                    participant.EligibilityPassed = true;
                    await _tournamentRepository.AddParticipant(participant);
                    tournament.Participants.Add(await _tournamentRepository.GetParticipantByIdAsync(participant.AccountId, participant.TournamentId));
                    await _tournamentRepository.UpdateAsync(tournament);
                    return true;
                }
                return false;
            }
            catch (Exception ex) { 
                return false;
            }

        }

        public async Task<ParticipationStatusDto> ParticipantStatustAsync(long id, long TournamentId)
        {
            try
            {
                var participant = await _tournamentRepository.GetParticipantByIdAsync(id,TournamentId);

                if (participant != null)
                {
                    return new ParticipationStatusDto
                    {
                        Joined = true,
                        JoinedOn = participant.JoinedOn,
                        Eligible = participant.EligibilityPassed,
                    };
                }

                return null;
            }
            catch (Exception ex) { 
                return null;
            }

        }

        public async Task<ICollection<LeaderboardDto>> GetLeaderBoardData(long tournamentId, int page, int pageSize, string sortBy)
        {
            var participants = await _tournamentRepository.GetTournamentPointsByIdAsync(tournamentId);

            var leaderBoardData = participants
                                 .GroupBy(p => p.AccountId)
                                 .Select(g => new
                                 {
                                     AccountId = g.Key,
                                     TotalPoints = g.Sum(x => x.Points),
                                     BestMultiplier = g.Max(x => x.Points)
                                 })
                                 .OrderByDescending(x => sortBy == "multiplier" ? x.BestMultiplier : x.TotalPoints)
                                 .Skip((page - 1) * pageSize).Take(pageSize)
                                 .Select((x, index) => new LeaderboardDto
                                 {
                                     Rank = (page - 1) * pageSize + index + 1,
                                     AccountId = x.AccountId,
                                     TotalPoints = (int)x.TotalPoints,
                                     BestMultiplier = x.BestMultiplier
                                 }).ToList();

            return leaderBoardData;
         }

        public async Task<ICollection<TournamentDto>> GetCurrentTournament()
        {
            var tournaments = await _tournamentRepository.GetAllTournament();

            var Result = tournaments.AsQueryable().Where(_ => _.StartDate <=  DateTime.Now && _.EndDate >= DateTime.Now && _.IsActive &&  !_.IsDeleted)
                               .Select(_ => new TournamentDto
                               {
                                   Name = _.Name,
                                   GameType = _.GameType,
                                   IsActive = _.IsActive,
                                   StartDate = _.StartDate,
                                   EndDate = _.EndDate,
                                   CreatedBy = _.CreatedBy,
                                   Description = _.Description,
                                   IsDeleted = _.IsDeleted,
                               }).ToList();
            return Result;
        }

        public async Task<ICollection<TournamentDto>> GetUpcomingTournament()
        {
            var tournaments = await _tournamentRepository.GetAllTournament();

            return tournaments.AsQueryable().Where(_ => _.StartDate >= DateTime.UtcNow && _.IsActive && !_.IsDeleted)
                                .OrderByDescending(x => x.StartDate)
                               .Select(_ => new TournamentDto
                               {
                                   Name = _.Name,
                                   GameType = _.GameType,
                                   IsActive = _.IsActive,
                                   StartDate = _.StartDate,
                                   EndDate = _.EndDate,
                                   CreatedBy = _.CreatedBy,
                                   Description = _.Description,
                                   IsDeleted = _.IsDeleted,
                               }).ToList();
        }
    }
}   