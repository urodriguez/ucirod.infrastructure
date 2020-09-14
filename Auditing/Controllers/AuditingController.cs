using Auditing.Infrastructure.Persistence;
using Shared.Infrastructure.CrossCutting.Authentication;
using Shared.Infrastructure.CrossCutting.Logging;
using Shared.WebApi.Controllers;

namespace Auditing.Controllers
{
    public class AuditingController : InfrastructureController
    {
        protected readonly AuditingDbContext _auditingDbContext;

        public AuditingController(
            AuditingDbContext auditingDbContext, 
            ICredentialService credentialService, 
            ILogService logService
        ) : base(
            credentialService,
            logService
        )
        {
            _auditingDbContext = auditingDbContext;
        }

        protected override void ConfigureLogging()
        {
            _logService.UseProject("Auditing");
        }
    }
}