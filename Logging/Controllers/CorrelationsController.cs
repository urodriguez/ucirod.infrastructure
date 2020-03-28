using Infrastructure.CrossCutting.Authentication;
using Logging.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Logging.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorrelationsController : LoggingController
    {
        private readonly ICorrelationService _correlationService;

        public CorrelationsController(ICorrelationService correlationService, ILogService logService, IConfiguration config) : base(logService, correlationService, config)
        {
            _correlationService = correlationService;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Account account)
        {
            return Execute(() =>
            {
                var correlationDto = _correlationService.Create(account);
                return Ok(correlationDto);
            });
        }
    }
}
