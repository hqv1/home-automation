using System.Threading.Tasks;
using Hqv.Thermostat.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hqv.Thermostat.Api.Controllers
{
    [Route("api/thermostat/v1/scene")]
    public class SceneController : Controller
    {
        private readonly IMediator _mediator;

        public SceneController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> AddScene([FromBody] SceneToAddModel model)
        {
            var result = await _mediator.Send(model);
            return Ok(result);
        }
    }
}