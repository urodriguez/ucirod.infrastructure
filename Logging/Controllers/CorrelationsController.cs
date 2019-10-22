using System;
using Logging.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Logging.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorrelationsController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post()
        {
            return Ok(new CorrelationDto());
        }
    }
}
