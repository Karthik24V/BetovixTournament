using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tournament.Common.Constants;
using Tournament.Common.Dto_s;

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
        public ActionResult PostTournament(TournamentRequestDto name)
        {
            _bus.Publish(name);
            return Ok();
        }
    }
}
