using Logging.Application;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Logging.Controllers
{
    public class CorrelationsController : LoggingController
    {
        private readonly ICorrelationService _correlationService;

        public CorrelationsController(ICorrelationService correlationService, ILogService logService) : base(logService)
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
