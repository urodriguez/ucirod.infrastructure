using Logging.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.CrossCutting.Authentication;

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
        public IActionResult Post([FromBody] Credential credential)
        {
            return Execute(() =>
            {
                var correlationDto = _correlationService.Create(credential);
                return Ok(correlationDto);
            });
        }
    }
}
