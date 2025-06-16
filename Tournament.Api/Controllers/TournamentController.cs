using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Business.IServices;
using Tournament.Common.DTOs;    
using Tournament.Common.Response;
using Microsoft.Extensions.Logging; // Added for logging
using static MassTransit.ValidationResultExtensions;

namespace Tournament.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TournamentController : ControllerBase
    {
        private readonly ITournamentService _tournamentService;
        private readonly ILogger<TournamentController> _logger; // Logger field

        // Constructor with logger injection
        public TournamentController(ITournamentService tournamentService, ILogger<TournamentController> logger)
        {
            _tournamentService = tournamentService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new tournament.
        /// </summary>
        /// <param name="dto">The tournament data.</param>
        /// <returns>The created tournament.</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> CreateTournament([FromBody] CreateTournamentDto dto)
        {
            _logger.LogInformation("CreateTournament called with Name: {Name}", dto.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid tournament data received.");
                return BadRequest(ApiResponse<TournamentDto>.FailureResponse("Invalid tournament data."));
            }

            var result = await _tournamentService.CreateTournamentAsync(dto);
            _logger.LogInformation("Tournament created with Id: {Id}", result.Id);

            return CreatedAtAction(nameof(GetTournamentById),
                new { id = result.Id },
                ApiResponse<TournamentDto>.SuccessResponse(result, "Tournament created successfully"));
        }

        /// <summary>
        /// Updates an existing tournament.
        /// </summary>
        /// <param name="id">Tournament Id.</param>
        /// <param name="dto">Updated tournament data.</param>
        /// <returns>The updated tournament.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> UpdateTournament(long id, [FromBody] UpdateTournamentDto dto)
        {
            _logger.LogInformation("UpdateTournament called for Id: {Id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid tournament data received for update.");
                return BadRequest(ApiResponse<TournamentDto>.FailureResponse("Invalid tournament data."));
            }

            var result = await _tournamentService.UpdateTournamentAsync(id, dto);
            if (result == null)
            {
                _logger.LogWarning("Tournament not found for update. Id: {Id}", id);
                return NotFound(ApiResponse<TournamentDto>.FailureResponse("Tournament not found."));
            }

            _logger.LogInformation("Tournament updated successfully. Id: {Id}", id);
            return Ok(ApiResponse<TournamentDto>.SuccessResponse(result, "Tournament updated successfully"));
        }

        /// <summary>
        /// Deletes a tournament by Id.
        /// </summary>
        /// <param name="id">Tournament Id.</param>
        /// <returns>Status of deletion.</returns>
        [HttpDelete("DeleteTournament")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteTournament(long id)
        {
            _logger.LogInformation("DeleteTournament called for Id: {Id}", id);

            var success = await _tournamentService.DeleteTournamentAsync(id);
            if (!success)
            {
                _logger.LogWarning("Tournament not found for deletion. Id: {Id}", id);
                return NotFound(ApiResponse<string>.FailureResponse("Tournament not found."));
            }

            _logger.LogInformation("Tournament deleted successfully. Id: {Id}", id);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Tournament deleted successfully"));
        }

        /// <summary>
        /// Gets a tournament by Id.
        /// </summary>
        /// <param name="id">Tournament Id.</param>
        /// <returns>The tournament data.</returns>
        [HttpGet("GetTournamentById")]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> GetTournamentById(long id)
        {
            _logger.LogInformation("GetTournamentById called for Id: {Id}", id);

            var result = await _tournamentService.GetTournamentByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Tournament not found. Id: {Id}", id);
                return NotFound(ApiResponse<TournamentDto>.FailureResponse("Tournament not found."));
            }

            _logger.LogInformation("Tournament fetched successfully. Id: {Id}", id);
            return Ok(ApiResponse<TournamentDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Joins a participant to a tournament.
        /// </summary>
        /// <param name="dto">Participant data.</param>
        /// <returns>Created participant data.</returns>
        [HttpPost("JoinTournament")]
        public async Task<ActionResult<ApiResponse<ParticipationDto>>> JoinTournament([FromBody] ParticipationDto dto)
        {
            _logger.LogInformation("JoinTournament called for TournamentId: {TournamentId}, AccountId: {AccountId}", dto.TournamentId, dto.AccountId);

            var success = await _tournamentService.JoinTournamentAsync(dto);

            if (!success)
            {
                _logger.LogWarning("Invalid participant for TournamentId: {TournamentId}, AccountId: {AccountId}", dto.TournamentId, dto.AccountId);
                return NotFound(ApiResponse<ParticipationDto>.FailureResponse("InValid participant"));
            }

            _logger.LogInformation("Participant joined successfully. TournamentId: {TournamentId}, AccountId: {AccountId}", dto.TournamentId, dto.AccountId);
            return Ok(ApiResponse<ParticipationDto>.SuccessResponse(dto, "Participant joined successfully"));
        }

        /// <summary>
        /// Checks the participant status in a tournament.
        /// </summary>
        /// <param name="ParticipantAccId">Participant Account Id.</param>
        /// <param name="TournamentId">Tournament Id.</param>
        /// <returns>Participation status data.</returns>
        [HttpGet("GetParticipantStatus")]
        public async Task<ActionResult<ApiResponse<ParticipationStatusDto>>> ParticipationStatus(int ParticipantAccId, int TournamentId)
        {
            _logger.LogInformation("ParticipationStatus called for ParticipantAccId: {ParticipantAccId}, TournamentId: {TournamentId}", ParticipantAccId, TournamentId);

            var result = await _tournamentService.ParticipantStatustAsync(ParticipantAccId, TournamentId);

            if (result == null)
            {
                _logger.LogWarning("Participant not found. ParticipantAccId: {ParticipantAccId}, TournamentId: {TournamentId}", ParticipantAccId, TournamentId);
                return NotFound(ApiResponse<ParticipationDto>.FailureResponse("Participant not found"));
            }

            _logger.LogInformation("Participant status fetched. ParticipantAccId: {ParticipantAccId}, TournamentId: {TournamentId}", ParticipantAccId, TournamentId);
            return Ok(ApiResponse<ParticipationStatusDto>.SuccessResponse(result, "Eligible participant"));
        }

        /// <summary>
        /// Gets the leaderboard for a tournament.
        /// </summary>
        /// <param name="tournamentId">Tournament Id.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="sortBy">Sort by field.</param>
        /// <returns>Leaderboard data.</returns>
        [HttpGet("leaderboard/{tournamentId}")]
        public async Task<IActionResult> GetLeaderboard(long tournamentId, int page = 1, int pageSize = 10, string sortBy = "points")
        {
            _logger.LogInformation("GetLeaderboard called for TournamentId: {TournamentId}, Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}", tournamentId, page, pageSize, sortBy);

            var Result = await _tournamentService.GetLeaderBoardData(tournamentId, page, pageSize, sortBy);

            if (Result == null || Result.Items.Count == 0)
            {
                _logger.LogWarning("Leaderboard data not found for TournamentId: {TournamentId}", tournamentId);
                return NotFound(ApiResponse<PagedResult<LeaderboardDto>>.FailureResponse("Invalid data"));
            }

            _logger.LogInformation("Leaderboard data fetched for TournamentId: {TournamentId}", tournamentId);
            return Ok(ApiResponse<PagedResult<LeaderboardDto>>.SuccessResponse(Result, "Fetched SuccessFully"));
        }

        /// <summary>
        /// Gets the current tournaments.
        /// </summary>
        /// <returns>Current tournaments data.</returns>
        [HttpGet("CurrentTournament")]
        public async Task<IActionResult> GetCurrentTournament()
        {
            _logger.LogInformation("GetCurrentTournament called.");

            var Result = await _tournamentService.GetCurrentTournament();

            if (Result == null)
            {
                _logger.LogWarning("No current tournaments found.");
                return NotFound(ApiResponse<ICollection<TournamentDto>>.FailureResponse("Invalid data"));
            }

            _logger.LogInformation("Current tournaments fetched.");
            return Ok(ApiResponse<ICollection<TournamentDto>>.SuccessResponse(Result, "Fetched SuccessFully"));
        }

        /// <summary>
        /// Gets the upcoming tournaments.
        /// </summary>
        /// <returns>Upcoming tournaments data.</returns>
        [HttpGet("UpComingTournament")]
        public async Task<IActionResult> GetUpComingTournament()
        {
            _logger.LogInformation("GetUpComingTournament called.");

            var Result = await _tournamentService.GetUpcomingTournament();

            if (Result == null)
            {
                _logger.LogWarning("No upcoming tournaments found.");
                return NotFound(ApiResponse<ICollection<TournamentDto>>.FailureResponse("Invalid data"));
            }

            _logger.LogInformation("Upcoming tournaments fetched.");
            return Ok(ApiResponse<ICollection<TournamentDto>>.SuccessResponse(Result, "Fetched SuccessFully"));
        }

        /// <summary>
        /// Gets the recent tournament winners.
        /// </summary>
        /// <param name="minCount">Minimum number of winners to return (default is 3).</param>
        /// <returns>List of recent tournament winners.</returns>
        [HttpGet("recent-winners")]
        public async Task<ActionResult<IList<TournamentWinnerDto>>> GetRecentTournamentWinners([FromQuery] int minCount = 3)
        {
            _logger.LogInformation("GetRecentTournamentWinners called with minCount: {MinCount}", minCount);

            var result = await _tournamentService.GetRecentTournamentWinnersAsync(minCount);

            if (result == null || result.Count == 0)
            {
                _logger.LogWarning("No recent tournament winners found for minCount: {MinCount}", minCount);
                return NotFound(ApiResponse<IList<TournamentWinnerDto>>.FailureResponse("No recent tournament winners found."));
            }

            _logger.LogInformation("Recent tournament winners fetched successfully. Count: {Count}", result.Count);
            return Ok(ApiResponse<IList<TournamentWinnerDto>>.SuccessResponse(result, "Recent tournament winners fetched successfully."));
        }
    }
}
