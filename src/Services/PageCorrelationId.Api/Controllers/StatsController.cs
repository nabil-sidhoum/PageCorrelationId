using System.Net.Mime;
using System.Threading.Tasks;
using PageCorrelationId.Application.Stats.Queries;
using PageCorrelationId.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PageCorrelationId.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class StatsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StatsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            StatsResult result = await _mediator.Send(new GetStatsQuery());
            return Ok(result);
        }
    }
}