using System;

namespace Hqv.Thermostat.Api.Models
{
    public class ModelBase
    {
        public ModelBase(string correlationId = null)
        {
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
        }

        public string CorrelationId { get; set; }
    }
}