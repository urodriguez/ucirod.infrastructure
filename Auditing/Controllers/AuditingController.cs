using Auditing.Infrastructure.Persistence;
using Core.WebApi;
using Infrastructure.CrossCutting.Authentication;
using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.Extensions.Configuration;

namespace Auditing.Controllers
{
    public class AuditingController : InfrastructureController
    {
        protected readonly AuditingDbContext _auditingDbContext;

        public AuditingController(
            AuditingDbContext auditingDbContext, 
            IClientService clientService, 
            ILogService logService,
            ICorrelationService correlationService,
            IConfiguration config
        ) : base(
            clientService,
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