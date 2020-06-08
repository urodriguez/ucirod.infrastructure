using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Rendering.Domain;
using Shared.Infrastructure.CrossCutting.AppSettings;
using Shared.Infrastructure.CrossCutting.Authentication;
using Shared.Infrastructure.CrossCutting.Logging;
using Shared.WebApi.Controllers;
using Template = Rendering.Domain.Template;

namespace Rendering.Controllers
{
    public class TemplatesController : InfrastructureController
    {
        private readonly IJsReportMVCService _jsReportMvcService;
        private readonly IAppSettingsService _appSettingsService;

        public TemplatesController(
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

            Directory.CreateDirectory($"{_appSettingsService.TemplatesRenderedDirectory}");
        }

        protected override void ConfigureLogging()
        {
            _logService.UseProject("Rendering");
        }

        [HttpPost]
        public async Task<IActionResult> Render([FromBody] TemplateDto templateDto)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;

            return await ExecuteAsync(templateDto.Credential,  async () =>
            {
                var acceptHeader = Request.Headers["Accept"].ToString();
                templateDto.OutputFormat = MapAcceptHeaderToTemplateOutputFormat(acceptHeader);

                #region ApplicationService-Layer
                var template = new Template(templateDto.Content, templateDto.DataBound, templateDto.OutputFormat);

                _logService.LogInfoMessageAsync($"{GetType().Name}.{methodName} | Redering template | credential.Id={templateDto.Credential.Id} - status=PENDING");
                var templateRendered = await _jsReportMvcService.RenderAsync(new RenderRequest
                {
                    Template = new jsreport.Types.Template
                    {
                        Recipe = GetRecipeType(template.OutputFormat),
                        Engine = Engine.JsRender,
                        Content = template.Content
                    },
                    Data = template.DataBound
                });
                _logService.LogInfoMessageAsync($"{GetType().Name}.{methodName} | Redering template | credential.Id={templateDto.Credential.Id} - status=FINISHED");

                var templateRenderedFileName = $"templateRendered_{templateDto.Credential.Id}_{Guid.NewGuid()}.{template.GetOutputExtension()}";

                _logService.LogInfoMessageAsync(
                    $"{GetType().Name}.{methodName} | " +
                    $"Creating templateRendered file | " +
                    $"credential.Id={templateDto.Credential.Id} - templateRenderedFileName={templateRenderedFileName}"
                );

                var templateRenderedFilePath = $"{_appSettingsService.TemplatesRenderedDirectory}\\{templateRenderedFileName}";
                using (var fs = System.IO.File.Create(templateRenderedFilePath))
                {
                    templateRendered.Content.CopyTo(fs);
                }

                Task.Run(async () => //delete file after 2 seconds, ensuring that the controller sent data to client
                {
                    if (!System.IO.File.Exists(templateRenderedFilePath)) return;
                    await Task.Delay(2000);
                    System.IO.File.Delete(templateRenderedFilePath);
                });
                #endregion

                //TemplateService (from ApplicationService-Layer) retuns templateRenderedFilePath

                return new PhysicalFileResult(templateRenderedFilePath, acceptHeader);
            });
        }

        private static OutputFormat MapAcceptHeaderToTemplateOutputFormat(string acceptHeader)
        {
            switch (acceptHeader)
            {
                case "application/pdf":
                    return OutputFormat.Pdf;
                case "text/html":
                    return OutputFormat.Html;
                default:
                    throw new ArgumentOutOfRangeException($"acceptHeader: {acceptHeader} not supported");
            }
        }

        private static Recipe GetRecipeType(OutputFormat outputFormat)
        {
            switch (outputFormat)
            {
                case OutputFormat.Pdf:
                    return Recipe.ChromePdf;
                case OutputFormat.Html:
                    return Recipe.Html;
                default:
                    throw new ArgumentOutOfRangeException($"outputFormat: {outputFormat} not supported");
            }
        }
    }
}
