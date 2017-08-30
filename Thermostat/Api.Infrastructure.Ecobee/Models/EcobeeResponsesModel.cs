namespace Hqv.Thermostat.Api.Infrastructure.Ecobee.Models
{
    public class EcobeeResponsesModel
    {
        public EcobeeResponsesModel(int code, string message)
        {
            Code = code;
            Message = message;
        }
        public int Code { get; }
        public string Message { get; }
    }
}