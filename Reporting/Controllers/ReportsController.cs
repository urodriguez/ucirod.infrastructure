using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.CrossCutting.AppSettings;
using Shared.Infrastructure.CrossCutting.Authentication;
using Shared.Infrastructure.CrossCutting.Logging;
using Shared.WebApi.Controllers;

namespace Reporting.Controllers
{
    public class ReportsController : InfrastructureController
    {
        private readonly IJsReportMVCService _jsReportMvcService;
        private readonly IAppSettingsService _appSettingsService;

        public ReportsController(
            ICredentialService credentialService, 
            ILogService logService,
            IJsReportMVCService jsReportMvcService,
            IAppSettingsService appSettingsService
        ) : base(
            credentialService, 
            logService
        )
        {
            _jsReportMvcService = jsReportMvcService;
            _appSettingsService = appSettingsService;
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

                _logService.LogInfoMessageAsync($"{GetType().Name}.{methodName}  | Redering report | credential.Id={reportDto.Credential.Id} - status=PENDING");
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
                _logService.LogInfoMessageAsync($"{GetType().Name}.{methodName}  | Redering report | credential.Id={reportDto.Credential.Id} - status=FINISHED");

                Directory.CreateDirectory($"{_appSettingsService.ReportsDirectory}");

                var reportFileName = $"report_{reportDto.Credential.Id}_{Guid.NewGuid()}.pdf";

                _logService.LogInfoMessageAsync($"{GetType().Name}.{methodName}  | Creating report file | credential.Id={reportDto.Credential.Id} - reportFileName={reportFileName}");

                using (var f = System.IO.File.Create($"{_appSettingsService.ReportsDirectory}\\{reportFileName}"))
                {
                    reportRendered.Content.CopyTo(f);
                }

                return new PhysicalFileResult($"{_appSettingsService.ReportsDirectory}\\{reportFileName}", "application/pdf");
            });
        }
    }
}
