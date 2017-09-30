using System;
using System.Linq;
using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain;

namespace Hqv.Thermostat.Api.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public static async Task<string> GetBearerToken(this IAuthenticationService authenticationService,
            string correlationId = null)
        {
            var response = await authenticationService.Authenticate(new AuthenticateRequest(correlationId));
            if (!response.IsValid)
                throw new AggregateException("Authentication service has errors", response.Errors);
            return response.BearerToken;
        }
    }
}
