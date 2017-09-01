using Dapper.Contrib.Extensions;
using Newtonsoft.Json;

namespace Hqv.Thermostat.Api.Infrastructure.Models
{
    [Table("Thermostat.EventLog")]
    internal class EventLogModel
    {
        [Key]
        public long EventLogID { get; set; }

        public string EntityName { get; set; }

        public string EventName { get; set; }

        public string Event { get; set; }

        public static EventLogModel ConvertFrom(Domain.Entities.EventLog entity)
        {
            var model = new EventLogModel
            {
                EntityName = entity.EntityName,
                EventName = entity.EventName,
                Event = JsonConvert.SerializeObject(entity)
            };
            return model;
        }       
    }
}