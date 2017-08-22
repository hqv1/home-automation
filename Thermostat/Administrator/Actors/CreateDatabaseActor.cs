using System;
using Hqv.CSharp.Common.Audit;
using Hqv.Thermostat.Administrator.Options;

namespace Hqv.Thermostat.Administrator.Actors
{
    internal class CreateDatabaseActor
    {
        private readonly IAuditor _auditor;
        private readonly ISystemRepository _systemRepository;

        public CreateDatabaseActor(IAuditor auditor, ISystemRepository systemRepository)
        {
            _auditor = auditor;
            _systemRepository = systemRepository;
        }

        public int Act(CreateDatabaseOptions options)
        {
            try
            {
                _systemRepository.CreateDatabase().Wait();
                _auditor.AuditSuccess(new BusinessEventDefault("Administrator", "Thermostat", "Database Created", DateTime.Now));
                return 0;
            }
            catch (Exception ex)
            {
                _auditor.AuditFailure(new BusinessEventDefault("Administrator", "Thermostat", "Database Creation Failed", DateTime.Now, additionalMetadata:ex));
            }
            return -1;
        }
    }
}
