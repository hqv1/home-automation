using System.Threading.Tasks;
using Hqv.Thermostat.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hqv.Thermostat.Api.Controllers
{
    [Route("api/thermostat/v1/thermostat/")]
    public class ThermostatController : Controller
    {
        private readonly IMediator _mediator;

        public ThermostatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("readings")]
        public async Task<IActionResult> GetReadings(ReadingToGet model)
        {           
            var thermostatReadings = await _mediator.Send(model);
            return Ok(thermostatReadings);
        }      
    }
}