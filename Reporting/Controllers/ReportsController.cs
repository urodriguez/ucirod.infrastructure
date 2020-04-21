using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.CrossCutting.Authentication;
using Shared.Infrastructure.CrossCutting.Logging;
using Shared.WebApi.Controllers;

namespace Reporting.Controllers
{
    public class ReportsController : InfrastructureController
    {
        // GET api/reports
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "report1", "report2" };
        }

        // GET api/reports/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "report" + id;
        }

        public ReportsController(ICredentialService credentialService, ILogService logService) : base(credentialService, logService)
        {
        }

        protected override void ConfigureLogging()
        {
            _logService.UseProject("Reporting");
        }
    }
}
