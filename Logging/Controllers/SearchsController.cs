using System;
using System.Reflection;
using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Logging.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchsController : ControllerBase
    {
        private readonly ILogService _logService;

        public SearchsController(ILogService logService, ICorrelationService correlationService, IConfiguration config)
        {
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
        public IActionResult Search([FromBody] LogSearchRequestDto logSearchRequestDto)
        {
            try
            {
                var logs = _logService.Search(logSearchRequestDto);

                return Ok(logs);
            }
            catch (UnauthorizedAccessException uae)
            {
                return Unauthorized();
            }
            catch (ArgumentOutOfRangeException argumentOutOfRangeException)
            {
                return BadRequest(argumentOutOfRangeException.Message);
            }
            catch (Exception e)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Exception | e.FullStackTrace={e}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}