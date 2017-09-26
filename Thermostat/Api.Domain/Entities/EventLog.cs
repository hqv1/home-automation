using System;
using Hqv.CSharp.Common.Audit;
using Hqv.CSharp.Common.Entities;

namespace Hqv.Thermostat.Api.Domain.Entities
{
    public class EventLog : BusinessEventDefault, IAggregateRoot
    {
        public EventLog(string entityName, string entityKey, string eventName, DateTime eventDateTime,
            string correlationId = null, int version = 1, object entityObject = null, object additionalMetadata = null)
            : base(entityName, entityKey, eventName, eventDateTime, correlationId, version, entityObject,
                additionalMetadata)
        {
        }
    }
}