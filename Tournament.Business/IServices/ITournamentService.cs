using Tournament.Common.DTOs;

namespace Tournament.Business.IServices
{
    public interface ITournamentService
    {
        Task<TournamentDto> CreateTournamentAsync(CreateTournamentDto dto);
        Task<TournamentDto> GetTournamentByIdAsync(long id);
        Task<TournamentDto> UpdateTournamentAsync(long id, UpdateTournamentDto dto);
        Task<bool> JoinTournamentAsync(ParticipationDto dto);
        Task<ParticipationStatusDto> ParticipantStatustAsync(long id, long TournamentId);
        Task<bool> DeleteTournamentAsync(long id); // New method for soft delete
    }
}