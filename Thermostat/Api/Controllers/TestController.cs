using System;
using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Hqv.Thermostat.Api.Controllers
{
    [Route("api/thermostat/v1/test/")]
    public class TestController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public TestController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate()
        {
            var correlationId = Guid.NewGuid().ToString();
            var request = new AuthenticateRequest(correlationId);
            var response = await _authenticationService.Authenticate(request);

            if (response.IsValid)           
                return NoContent();

            return StatusCode(500, "An unexpected fault happened. Try again later");
        }
    }
}