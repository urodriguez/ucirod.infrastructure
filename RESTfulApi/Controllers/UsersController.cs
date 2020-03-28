using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace RESTfulApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpPost]
        public IActionResult Register([FromBody] UserRegisterDto userRegisterDto)
        {
            //ref: https://support.twilio.com/hc/en-us/articles/223136027-Auth-Tokens-and-How-to-Change-Them
            
            //store userRegisterDto

            return Ok(new Credential
            {
                Id = "InventApp",
                SecretKey = "1nfr4structur3_1nv3nt4pp"
            });
        }
    }
}
