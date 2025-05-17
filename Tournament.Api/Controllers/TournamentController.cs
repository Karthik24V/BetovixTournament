using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Business.IServices;
using Tournament.Common.DTOs;
using Tournament.Common.Response;
using static MassTransit.ValidationResultExtensions;

namespace Tournament.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TournamentController : ControllerBase
    {
        private readonly ITournamentService _tournamentService;

        public TournamentController(ITournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }

        /// <summary>
        /// Creates a new tournament.
        /// </summary>
        /// <param name="dto">The tournament data.</param>
        /// <returns>The created tournament.</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> CreateTournament([FromBody] CreateTournamentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<TournamentDto>.FailureResponse("Invalid tournament data."));

            var result = await _tournamentService.CreateTournamentAsync(dto);
            return CreatedAtAction(nameof(GetTournamentById),
                new { id = result.Id },
                ApiResponse<TournamentDto>.SuccessResponse(result, "Tournament created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> UpdateTournament(long id, [FromBody] UpdateTournamentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<TournamentDto>.FailureResponse("Invalid tournament data."));

            var result = await _tournamentService.UpdateTournamentAsync(id, dto);
            if (result == null)
                return NotFound(ApiResponse<TournamentDto>.FailureResponse("Tournament not found."));

            return Ok(ApiResponse<TournamentDto>.SuccessResponse(result, "Tournament updated successfully"));
        }

        [HttpDelete("DeleteTournament")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteTournament(long id)
        {
            var success = await _tournamentService.DeleteTournamentAsync(id);
            if (!success)
                return NotFound(ApiResponse<string>.FailureResponse("Tournament not found."));

            return Ok(ApiResponse<string>.SuccessResponse(null, "Tournament deleted successfully"));
        }

        [HttpGet("GetTournamentById")]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> GetTournamentById(long id)
        {
            var result = await _tournamentService.GetTournamentByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<TournamentDto>.FailureResponse("Tournament not found."));

            return Ok(ApiResponse<TournamentDto>.SuccessResponse(result));
        }

        /// <summary>
        /// join the participant to tournament
        /// </summary>
        /// <param name="dto">participant data</param>
        /// <returns>Created participant data</returns>
        [HttpPost("JoinTournament")]
        public async Task<ActionResult<ApiResponse<ParticipationDto>>> JoinTournament([FromBody]ParticipationDto dto)
        {
            var success = await _tournamentService.JoinTournamentAsync(dto);

            if (!success)
                return NotFound(ApiResponse<ParticipationDto>.FailureResponse("InValid participant"));

            return Ok(ApiResponse<ParticipationDto>.SuccessResponse(dto, "Participant joined successfully"));
        }

        /// <summary>
        /// Check the participant status
        /// </summary>
        /// <param name="ParticipantAccId"></param>
        /// <param name="TournamentId"></param>
        /// <returns>ParticipationStatusDto</returns>
        [HttpGet("GetParticipantStatus")]
        public async Task<ActionResult<ApiResponse<ParticipationStatusDto>>> ParticipationStatus(int ParticipantAccId , int TournamentId)
        {
            var result = await _tournamentService.ParticipantStatustAsync(ParticipantAccId, TournamentId);

            if (result == null)
                return NotFound(ApiResponse<ParticipationDto>.FailureResponse("Participant not found"));

            return Ok(ApiResponse<ParticipationStatusDto>.SuccessResponse(result, "Eligible participant"));
        }

        [HttpGet("leaderboard/{tournamentId}")]
        public async Task<IActionResult> GetLeaderboard(long tournamentId, int page = 1, int pageSize = 10, string sortBy = "points")
        {
              var Result = await _tournamentService.GetLeaderBoardData(tournamentId, page, pageSize, sortBy);

                if (Result == null)
                    return NotFound(ApiResponse<ICollection<LeaderboardDto>>.FailureResponse("Invalid data"));

                return Ok(ApiResponse<ICollection<LeaderboardDto>>.SuccessResponse(Result, "Fetched SuccessFully"));
        }

        [HttpGet("CurrentTournament")]
        public async Task<IActionResult> GetCurrentTournament()
        {
            var Result = await _tournamentService.GetCurrentTournament();

            if (Result == null)
                return NotFound(ApiResponse<ICollection<TournamentDto>>.FailureResponse("Invalid data"));

            return Ok(ApiResponse<ICollection<TournamentDto>>.SuccessResponse(Result, "Fetched SuccessFully"));
        }

        [HttpGet("UpComingTournament")]
        public async Task<IActionResult> GetUpComingTournament()
        {
            var Result = await _tournamentService.GetUpcomingTournament();

            if (Result == null)
                return NotFound(ApiResponse<ICollection<TournamentDto>>.FailureResponse("Invalid data"));

            return Ok(ApiResponse<ICollection<TournamentDto>>.SuccessResponse(Result, "Fetched SuccessFully"));
        }
    }
}
