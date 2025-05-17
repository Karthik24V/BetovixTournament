using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tournament.Common.Constants;
using Tournament.Common.Dto_s;
using Tournament.Common.Response;

namespace Tournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProducerController : ControllerBase
    {
        private readonly IBus _bus;
        public ProducerController(IBus bus) { 
            _bus = bus;
        }

        [Authorize(AuthenticationSchemes = AuthenticationSchemeConstants.ApiKey)]
        [HttpPost]
        public ActionResult PostTournamentEvent(TournamentEventDto dto)
        {
            _bus.Publish(dto);
            return Ok(ApiResponse<TournamentEventDto>.SuccessResponse(dto,"Event Published Successfully"));
        }
    }
}
