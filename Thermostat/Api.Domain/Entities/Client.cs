namespace Hqv.Thermostat.Api.Domain.Entities
{
    public class Client
    {
        public Client(long clientId, string name, ClientAuthentication authentication = null)
        {
            ClientId = clientId;
            Name = name;
            Authentication = authentication;
        }

        public long ClientId { get; }
        public string Name { get; }
        public ClientAuthentication Authentication { get; set; }
    }
}