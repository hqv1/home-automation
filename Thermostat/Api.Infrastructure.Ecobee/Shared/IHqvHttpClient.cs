using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared
{
    public interface IHqvHttpClient
    {
        Task<TResult> GetAsyncWithBearerToken<TResult>(
            string baseUri, string relativeUri, 
            string bearerToken,
            Func<object, Task<TResult>> parser, 
            ICollection<KeyValuePair<string, string>> queryParameters = null);

        Task<TResult> PostAsyncJsonWithBearerToken<TResult>(
            string baseUri,
            string relativeUri,
            string bearerToken,
            string body,
            Func<object, Task<TResult>> parser,
            ICollection<KeyValuePair<string, string>> queryParameters = null);
    }
}