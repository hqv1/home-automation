using System;
using Hqv.CSharp.Common.Audit;

namespace Hqv.Thermostat.Api.Domain.Entities
{
    public class EventLog : BusinessEventDefault
    {
        public EventLog(string entityName, string entityKey, string eventName, DateTime eventDateTime,
            string correlationId = null, int version = 1, object entityObject = null, object additionalMetadata = null)
            : base(entityName, entityKey, eventName, eventDateTime, correlationId, version, entityObject,
                additionalMetadata)
        {
        }
    }
}