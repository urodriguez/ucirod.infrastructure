using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Authentication.Domain;
using Authentication.Dtos;
using Infrastructure.CrossCutting.Authentication;
using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogService _logService;

        public TokensController(IClientService clientService, ILogService logService, ICorrelationService correlationService, IConfiguration config)
        {
            _clientService = clientService;

            _logService = logService;
            _logService.Configure(new LogSettings
            {
                Application = "Infrastructure",
                Project = "Authentication",
                Environment = config.GetValue<string>("Environment"),
                CorrelationId = correlationService.Create(null, false).Id
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] TokenCreateDto tokenCreateDto)
        {
            try
            {
                if (!_clientService.CredentialsAreValid(tokenCreateDto.Account))
                {
                    if (tokenCreateDto.Account == null) throw new ArgumentNullException("Credentials not provided");
                    throw new UnauthorizedAccessException();
                }
                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | account.Id={tokenCreateDto.Account.Id}");

                // create a claimsIdentity
                var claimsIdentity = new ClaimsIdentity(tokenCreateDto.Claims.Select(c => new Claim(c.Type, c.Value)));

                // create token to the user
                var tokenHandler = new JwtSecurityTokenHandler();

                // describe security token
                var securityTokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = UciRodToken.Issuer,
                    Subject = claimsIdentity,
                    NotBefore = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(tokenCreateDto.Expires ?? UciRodToken.Expires)),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.Default.GetBytes(tokenCreateDto.Account.SecretKey)), SecurityAlgorithms.HmacSha256Signature)
                };

                // create JWT security token based on descriptor
                var jwtSecurityToken = tokenHandler.CreateJwtSecurityToken(securityTokenDescriptor);
                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | JWT security token created successfully | account.Id={tokenCreateDto.Account.Id}");

                return Ok(new
                {
                    UciRodToken.Issuer,
                    securityTokenDescriptor.Expires,
                    Token = tokenHandler.WriteToken(jwtSecurityToken) //security token as string
                });
            }
            catch (UnauthorizedAccessException uae)
            {
                return Unauthorized();
            }
            catch (ArgumentNullException ane)
            {
                return BadRequest(ane.Message);
            }
            catch (Exception e)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Exception | e.FullStackTrace={e}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [Route("validate")]
        [HttpPost]
        public IActionResult Validate([FromBody] TokenValidateDto tokenValidateDto)
        {
            try
            {
                if (!_clientService.CredentialsAreValid(tokenValidateDto.Account))
                {
                    if (tokenValidateDto.Account == null) throw new ArgumentNullException("Credentials not provided");
                    throw new UnauthorizedAccessException();
                }
                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | account.Id={tokenValidateDto.Account.Id}");

                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = UciRodToken.Issuer,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    LifetimeValidator = LifetimeValidator,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(tokenValidateDto.Account.SecretKey)),
                    ValidateAudience = false
                };

                var identity = tokenHandler.ValidateToken(tokenValidateDto.Token, validationParameters, out var validatedToken);
                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | JWT security token validated successfully | account.Id={tokenValidateDto.Account.Id}");

                var internalClaimTypes = new[] { "nbf", "exp", "iat", "iss" };

                var claims = identity.Claims.Where(c => !internalClaimTypes.Contains(c.Type));
                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | account.Id={tokenValidateDto.Account.Id} - claims.Count={claims.Count()}");

                return Ok(new
                {
                    TokenStatus = TokenStatus.Valid,
                    Claims = claims.Select(c => new
                    {
                        c.Type,
                        c.Value
                    })
                });
            }
            catch (UnauthorizedAccessException uae)
            {
                return Unauthorized();
            }
            catch (ArgumentNullException ane)
            {
                return BadRequest(ane.Message);
            }
            catch (SecurityTokenInvalidLifetimeException stile)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | SecurityTokenInvalidLifetimeException | account.Id={tokenValidateDto.Account.Id}");
                return Ok(new
                {
                    TokenStatus = TokenStatus.Expirated,
                });
            }
            catch (Exception e)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Exception | account.Id={tokenValidateDto.Account.Id} - fullStackTrace={e}");
                return Ok(new
                {
                    TokenStatus = TokenStatus.Invalid,
                });
            }
        }

        private bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (expires == null) return false;

            return DateTime.UtcNow < expires;
        }
    }
}
