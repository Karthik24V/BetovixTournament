using Microsoft.AspNetCore.Mvc;
using Tournament.Business.IServices;
using Tournament.Common.DTOs;
using Tournament.Common.Response;

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

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteTournament(long id)
        {
            var success = await _tournamentService.DeleteTournamentAsync(id);
            if (!success)
                return NotFound(ApiResponse<string>.FailureResponse("Tournament not found."));

            return Ok(ApiResponse<string>.SuccessResponse(null, "Tournament deleted successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> GetTournamentById(long id)
        {
            var result = await _tournamentService.GetTournamentByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<TournamentDto>.FailureResponse("Tournament not found."));

            return Ok(ApiResponse<TournamentDto>.SuccessResponse(result));
        }
    }
}
