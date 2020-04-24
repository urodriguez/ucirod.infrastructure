using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.CrossCutting.Authentication;
using Shared.Infrastructure.CrossCutting.Logging;
using Shared.WebApi.Controllers;

namespace Reporting.Controllers
{
    public class ReportsController : InfrastructureController
    {
        private readonly IJsReportMVCService _jsReportMvcService;

        public ReportsController(ICredentialService credentialService, ILogService logService, IJsReportMVCService jsReportMvcService) : base(credentialService, logService)
        {
            _jsReportMvcService = jsReportMvcService;
        }

        protected override void ConfigureLogging()
        {
            _logService.UseProject("Reporting");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReportDto reportDto)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;

            return await ExecuteAsync(reportDto.Credential,  async () =>
            {
                var report = new Domain.Report(reportDto.Template, reportDto.Data);

                _logService.LogInfoMessage($"{GetType().Name}.{methodName}  | Redering report | credential.Id={reportDto.Credential.Id} - status=PENDING");
                var reportRendered = await _jsReportMvcService.RenderAsync(new RenderRequest
                {
                    Template = new Template
                    {
                        Recipe = Recipe.ChromePdf,
                        Engine = Engine.JsRender,
                        Content = report.Template
                    },
                    Data = report.Data
                });
                _logService.LogInfoMessage($"{GetType().Name}.{methodName}  | Redering report | credential.Id={reportDto.Credential.Id} - status=FINISHED");

                using (var ms = new MemoryStream())
                {
                    reportRendered.Content.CopyTo(ms);
                    _logService.LogInfoMessage($"{GetType().Name}.{methodName}  | Report as bite array built | credential.Id={reportDto.Credential.Id} - ms.Length={ms.Length}");

                    return Ok(ms.ToArray());
                }
            });
        }
    }
}
