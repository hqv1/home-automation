using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Exceptions;
using Newtonsoft.Json;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared
{
    public class HqvHttpClient : IHqvHttpClient
    {
        private readonly HttpClient _httpClient;

        public HqvHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<TResult> GetAsyncWithBearerToken<TResult>(
            string baseUri, 
            string relativeUri, 
            string bearerToken, 
            Func<object, Task<TResult>> parser, 
            ICollection<KeyValuePair<string, string>> queryParameters = null)
        {
            var uri = UriHelper.Create(baseUri, relativeUri, queryParameters);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(uri);
            }
            catch (Exception ex)
            {
                ex.Data["uri"] = uri;
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var exception = new HqvException($"Failed with error code {response.StatusCode}");
                exception.Data["uri"] = uri;
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }

            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();                
                var json = JsonConvert.DeserializeObject(responseContent);               
                var result = await parser(json);
                return result;
            }
            catch (Exception ex)
            {
                ex.Data["uri"] = uri;
                ex.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw;
            }
        }

        public async Task<TResult> PostAsyncJsonWithBearerToken<TResult>(
            string baseUri,
            string relativeUri,
            string bearerToken,
            string body,
            Func<object, Task<TResult>> parser,
            ICollection<KeyValuePair<string, string>> queryParameters = null)
        {
            var uri = UriHelper.Create(baseUri, relativeUri, queryParameters);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(uri, content);
            }
            catch (Exception ex)
            {                
                ex.Data["uri"] = uri;
                ex.Data["body"] = body;
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var exception = new HqvException($"Failed with error code {response.StatusCode}");
                exception.Data["uri"] = uri;
                exception.Data["body"] = body;
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }

            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject(responseContent);
                var result = await parser(json);
                return result;
            }
            catch (Exception ex)
            {              
                ex.Data["uri"] = uri;
                ex.Data["body"] = body;
                ex.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw;
            }
        }
    }
}