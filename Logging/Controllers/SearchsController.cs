using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Logging.Controllers
{
    public class SearchsController : LoggingController
    {
        public SearchsController(ILogService logService) : base(logService)
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