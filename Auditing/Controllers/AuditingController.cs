using Auditing.Infrastructure.Persistence;
using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.CrossCutting.Authentication;
using Shared.WebApi.Controllers;

namespace Auditing.Controllers
{
    public class AuditingController : InfrastructureController
    {
        protected readonly AuditingDbContext _auditingDbContext;

        public AuditingController(
            AuditingDbContext auditingDbContext, 
            ICredentialService credentialService, 
            ILogService logService,
            ICorrelationService correlationService,
            IConfiguration config
        ) : base(
            credentialService,
            logService
        )
        {
            _auditingDbContext = auditingDbContext;
            _logService.Configure(new LogSettings
            {
                Application = "Infrastructure",
                Project = "Auditing",
                Environment = config.GetValue<string>("Environment"),
                CorrelationId = correlationService.Create(null, false).Id
            });
        }
    }
}