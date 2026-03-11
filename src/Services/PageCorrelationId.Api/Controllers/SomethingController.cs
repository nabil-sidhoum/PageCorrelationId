using System.Net.Mime;
using System.Threading.Tasks;
using PageCorrelationId.Application.Something.Queries;
using PageCorrelationId.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PageCorrelationId.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class SomethingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SomethingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetSomething()
        {
            SomethingResult result = await _mediator.Send(new GetSomethingQuery());
            return Ok(result);
        }
    }
}