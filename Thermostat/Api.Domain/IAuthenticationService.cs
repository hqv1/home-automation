using System;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Components;

namespace Hqv.Thermostat.Api.Domain
{
    public interface IAuthenticationService
    {
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest request);
    }

    public class AuthenticateRequest : RequestBase
    {
        public AuthenticateRequest(string correlationId = null)
            : base(correlationId ?? Guid.NewGuid().ToString())
        {
        }
    }

    public class AuthenticateResponse : ResponseBase
    {
        public AuthenticateResponse(AuthenticateRequest request)
            : base(request)
        {
        }

        public long ClientId { get; set; }
        public string BearerToken { get; set; }        
    }
}