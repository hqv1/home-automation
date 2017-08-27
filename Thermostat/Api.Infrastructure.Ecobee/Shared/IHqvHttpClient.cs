using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared
{
    public interface IHqvHttpClient
    {
        System.Threading.Tasks.Task<TResult> GetAsyncWithBearerToken<TResult>(
            string baseUri, 
            string relativeUri,
            string bearerToken,
            System.Func<dynamic, TResult> parser,
            ICollection<KeyValuePair<string, string>> queryParameters = null, 
            string correlationId = null,
            [CallerMemberName] string memberName = "");
    }
}