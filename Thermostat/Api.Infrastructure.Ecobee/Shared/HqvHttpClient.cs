using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Exceptions;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;
using Newtonsoft.Json;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared
{
    public class HqvHttpClient : IHqvHttpClient
    {
        public class Settings
        {
            public Settings(bool shouldLogResponse)
            {
                ShouldLogResponse = shouldLogResponse;
            }

            public bool ShouldLogResponse { get; }
        }

        private readonly IEventLogRepository _eventLogRepository;
        private readonly Settings _settings;

        private readonly HttpClient _httpClient;

        public HqvHttpClient(IEventLogRepository eventLogRepository, Settings settings)
        {
            _eventLogRepository = eventLogRepository;
            _settings = settings;
            _httpClient = new HttpClient();
        }

        public async Task<TResult> GetAsyncWithBearerToken<TResult>(
            string baseUri,
            string relativeUri,
            string bearerToken,
            Func<dynamic, TResult> parser,
            ICollection<KeyValuePair<string, string>> queryParameters = null,
            string correlationId = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
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
                if (_settings.ShouldLogResponse)
                {
                    await _eventLogRepository.Add(new EventLog(
                        "Ecobee", relativeUri, "ResponseBody", DateTime.UtcNow, correlationId, entityObject: json));
                }
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