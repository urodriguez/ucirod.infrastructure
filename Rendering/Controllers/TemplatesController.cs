using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Rendering.Domain;
using Shared.Application.Exceptions;
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
        public async Task<IActionResult> RenderAsync([FromBody] TemplateDto templateDto)
        {
            const string methodName = "RenderAsync";

            return await ExecuteAsync(templateDto.Credential,  async () =>
            {
                #region ApplicationService-Layer
                var template = new Template(templateDto.Content, templateDto.DataBound, templateDto.Type, templateDto.RenderAs);

                _logService.LogInfoMessageAsync($"{GetType().Name}.{methodName} | Redering template | credential.Id={templateDto.Credential.Id} - status=PENDING");
                var templateRendered = await _jsReportMvcService.RenderAsync(new RenderRequest
                {
                    Template = new jsreport.Types.Template
                    {
                        Recipe = GetRecipeType(template.Type),
                        Engine = Engine.JsRender,
                        Content = template.Content
                    },
                    Data = template.DataBound
                });
                _logService.LogInfoMessageAsync($"{GetType().Name}.{methodName} | Redering template | credential.Id={templateDto.Credential.Id} - status=FINISHED");

                var templateRenderedFileName = $"templateRendered_{templateDto.Credential.Id}_{Guid.NewGuid()}.{template.GetFileExtension()}";

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

                return template.RenderAs == RenderAs.Bytes 
                    ? new PhysicalFileResult(templateRenderedFilePath, GetAcceptHeader(template.Type)) as IActionResult
                    : Ok(await System.IO.File.ReadAllTextAsync(templateRenderedFilePath)) as IActionResult;
            });
        }

        private static string GetAcceptHeader(TemplateType templateType)
        {
            switch (templateType)
            {
                case TemplateType.Pdf:
                    return "application/pdf";
                case TemplateType.Html:
                    return "text/html";
                case TemplateType.PlainText:
                    return "text/plain";
                default://is internal error due to the data provided for client was previous validated at Domain Layer
                    throw new InternalServerException($"Unable to get Accept Header for templateType={templateType}");
            }
        }

        private static Recipe GetRecipeType(TemplateType templateType)
        {
            switch (templateType)
            {
                case TemplateType.Pdf:
                    return Recipe.ChromePdf;
                case TemplateType.Html:
                    return Recipe.Html;                
                case TemplateType.PlainText:
                    return Recipe.Text;
                default://is internal error due to the data provided for client was previous validated at Domain Layer
                    throw new InternalServerException($"Unable to get JsReport.Recipe type for templateType={templateType}");
            }
        }
    }
}
