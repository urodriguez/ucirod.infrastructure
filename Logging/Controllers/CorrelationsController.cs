using System;
using System.Reflection;
using Infrastructure.CrossCutting.Authentication;
using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Logging.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorrelationsController : ControllerBase
    {
        private readonly ICorrelationService _correlationService;
        private readonly ILogService _logService;

        public CorrelationsController(ICorrelationService correlationService, ILogService logService, IConfiguration config)
        {
            _correlationService = correlationService;

            _logService = logService;
            _logService.Configure(new LogSettings
            {
                Application = "Infrastructure",
                Project = "Logging",
                Environment = config.GetValue<string>("Environment"),
                CorrelationId = correlationService.Create(null, false).Id
            });
        }

        [HttpPost]
        public IActionResult Post([FromBody] Account account)
        {
            try
            {
                var correlationDto = _correlationService.Create(account);
                return Ok(correlationDto);
            }
            catch (UnauthorizedAccessException uae)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Exception | e.FullStackTrace={e}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
