using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Authentication.Domain;
using Authentication.Dtos;
using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Infrastructure.CrossCutting.Authentication;
using Shared.WebApi.Controllers;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    public class TokensController : InfrastructureController
    {
        public TokensController(ICredentialService credentialService, ILogService logService, ICorrelationService correlationService, IConfiguration config) : base(credentialService, logService)
        {
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
            return Execute(tokenCreateDto.Credential, () =>
            {
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
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.Default.GetBytes(tokenCreateDto.Credential.SecretKey)), SecurityAlgorithms.HmacSha256Signature)
                };

                // create JWT security token based on descriptor
                var jwtSecurityToken = tokenHandler.CreateJwtSecurityToken(securityTokenDescriptor);
                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | JWT security token created successfully | credential.Id={tokenCreateDto.Credential.Id}");

                return Ok(new
                {
                    UciRodToken.Issuer,
                    securityTokenDescriptor.Expires,
                    SecurityToken = tokenHandler.WriteToken(jwtSecurityToken) //security token as string
                });
            });
        }

        [Route("validate")]
        [HttpPost]
        public IActionResult Validate([FromBody] TokenValidateDto tokenValidateDto)
        {
            return Execute(tokenValidateDto.Credential, () =>
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();

                    var validationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = UciRodToken.Issuer,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        LifetimeValidator = LifetimeValidator,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(tokenValidateDto.Credential.SecretKey)),
                        ValidateAudience = false
                    };

                    var identity = tokenHandler.ValidateToken(tokenValidateDto.SecurityToken, validationParameters, out var validatedToken);
                    _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | JWT security token validated successfully | credential.Id={tokenValidateDto.Credential.Id}");

                    var internalClaimTypes = new[] { "nbf", "exp", "iat", "iss" };

                    var claims = identity.Claims.Where(c => !internalClaimTypes.Contains(c.Type));
                    _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | credential.Id={tokenValidateDto.Credential.Id} - claims.Count={claims.Count()}");

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
                catch (SecurityTokenInvalidLifetimeException stile)
                {
                    _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | SecurityTokenInvalidLifetimeException | credential.Id={tokenValidateDto.Credential.Id}");
                    return Ok(new
                    {
                        TokenStatus = TokenStatus.Expired
                    });
                }
                catch (Exception e)//overrides generic Exception catching to catch exceptions from: tokenHandler.ValidateToken
                {
                    _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Exception | credential.Id={tokenValidateDto.Credential.Id} - fullStackTrace={e}");
                    return Ok(new
                    {
                        TokenStatus = TokenStatus.Invalid
                    });
                }
            });
        }

        private bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (expires == null) return false;

            return DateTime.UtcNow < expires;
        }
    }
}
