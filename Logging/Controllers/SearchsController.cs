using System.Threading.Tasks;
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
        public async Task<IActionResult> SearchAsync([FromBody] LogSearchRequestDto logSearchRequestDto)
        {
            return await ExecuteAsync(async () =>
            {
                var logs = await _logService.SearchAsync(logSearchRequestDto);

                return Ok(logs);
            });
        }
    }
}