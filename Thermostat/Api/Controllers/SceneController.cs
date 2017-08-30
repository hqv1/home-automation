using System;
using System.Threading.Tasks;
using Hqv.Thermostat.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hqv.Thermostat.Api.Controllers
{
    [Route("api/thermostat/v1/scenes")]
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

        [HttpDelete("all")]
        public async Task<IActionResult> RemoveAllScenes()
        {
            throw new NotImplementedException();
        }
    }
}