using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Exceptions;
using Newtonsoft.Json;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared
{
    public static class HttpClientExtension
    {
        public static async Task<TResult> GetAsyncParsed<TResult>(
            this HttpClient client,
            string baseUri,
            string relativeUri,         
            string bearerToken,
            Func<dynamic,TResult> parser,
            ICollection<KeyValuePair<string, string>> queryParameters = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            var uri = UriHelper.Create(baseUri, relativeUri, queryParameters);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(uri);
            }
            catch (Exception ex)
            {
                var exception = new HqvException($"{memberName} failed.", ex);
                exception.Data["uri"] = uri;
                throw exception;
            }

            if (!response.IsSuccessStatusCode)
            {
                var exception =
                    new HqvException($"{memberName} failed with error code {response.StatusCode}");
                exception.Data["uri"] = uri;
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }

            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(responseContent);
                var result = parser(json);
                return result;
            }
            catch (Exception ex)
            {
                var exception = new HqvException($"Unable to parse response for {memberName}", ex);
                exception.Data["uri"] = uri;
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }           
        }
        
    }
}