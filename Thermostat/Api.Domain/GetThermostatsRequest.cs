namespace Hqv.Thermostat.Api.Domain
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

        public bool IncludeReadings { get; set; }
        public bool IncludeSettings { get; set; }
        public bool IncludeScenes { get; set; }

    }
}