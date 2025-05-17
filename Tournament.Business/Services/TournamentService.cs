using Tournament.Common.DTOs;
using Tournament.Business.IServices;
using Tournament.Data.IRepository;
using Tournament.Domain.DataBase.Entity;
using Tournament.Data.Repository;
using AutoMapper;

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
    }
}