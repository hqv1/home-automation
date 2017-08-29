using System.Threading.Tasks;
using Hqv.Thermostat.Api.Messages;
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
        public async Task<IActionResult> GetReadings()
        {
            var thermostatReadings = await _mediator.Send(new GetThermostatReadingMessage());
            return Ok(thermostatReadings);
        }      
    }

    [Route("api/thermostat/v1/hold")]
    public class HoldController : Controller
    {
        private readonly IMediator _mediator;

        public HoldController(IMediator mediator)
        {
            _mediator = mediator;
        }

        
    }
}