using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Logging.Controllers
{
    [Route("api/[controller]")]
    public class LogsController : LoggingController
    {
        public LogsController(ILogService logService) : base(logService)
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
