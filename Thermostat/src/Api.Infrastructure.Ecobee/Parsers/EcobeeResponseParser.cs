using Hqv.Thermostat.Api.Infrastructure.Ecobee.Models;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee.Parsers
{
    public static class EcobeeResponseParser
    {
        public static EcobeeResponsesModel Parse(dynamic json)
        {
            var statusJson = json.status;
            var code = (int) statusJson.code;
            var message = (string) statusJson.message;
            return new EcobeeResponsesModel(code, message);
        }
    }
}