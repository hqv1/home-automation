using Microsoft.AspNetCore.Mvc;

namespace Hqv.Thermostat.Api.Controllers
{
    [Route("api/thermostat/v1/health")]
    public class HealthController : Controller
    {       
        [HttpGet]
        [HttpHead]
        public IActionResult Get()
        {     
            return Ok("Normal");
        }
    }
}
