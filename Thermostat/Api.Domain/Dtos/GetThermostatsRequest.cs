namespace Hqv.Thermostat.Api.Domain.Dtos
{
    public class GetThermostatsRequest
    {
        public GetThermostatsRequest(bool includeReadings, bool includeSettings, bool includeScenes)
        {
            IncludeReadings = includeReadings;
            IncludeSettings = includeSettings;
            IncludeScenes = includeScenes;
        }

        public string BearerToken { get; set; }
        public string CorrelationId { get; set; }

        public bool IncludeReadings { get; }
        public bool IncludeSettings { get; }
        public bool IncludeScenes { get; }

    }
}