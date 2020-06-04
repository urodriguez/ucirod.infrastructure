using System.Threading.Tasks;
using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Logging.Controllers
{
    public class LogsController : LoggingController
    {
        public LogsController(ILogService logService) : base(logService)
        {
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] LogDtoPost logDto)
        {
            return await ExecuteAsync(async () =>
            {
                await _logService.LogAsync(logDto);
                return Ok();
            });
        }
    }
}
