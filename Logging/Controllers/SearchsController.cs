using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Logging.Controllers
{
    [Route("api/[controller]")]
    public class SearchsController : LoggingController
    {
        public SearchsController(ILogService logService, ICorrelationService correlationService, IConfiguration config) : base(logService, correlationService, config)
        {
        }

        [HttpPost]
        public IActionResult Search([FromBody] LogSearchRequestDto logSearchRequestDto)
        {
            return Execute(() =>
            {
                var logs = _logService.Search(logSearchRequestDto);

                return Ok(logs);
            });
        }
    }
}