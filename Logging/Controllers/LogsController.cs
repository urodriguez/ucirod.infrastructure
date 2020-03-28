using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Logging.Controllers
{
    [Route("api/[controller]")]
    public class LogsController : LoggingController
    {
        public LogsController(ILogService logService, ICorrelationService correlationService, IConfiguration config) : base(logService, correlationService, config)
        {
        }

        [HttpPost]
        public IActionResult Post([FromBody] LogDtoPost logDto)
        {
            return Execute(() =>
            {
                _logService.Log(logDto);
                return Ok();
            });
        }
    }
}
