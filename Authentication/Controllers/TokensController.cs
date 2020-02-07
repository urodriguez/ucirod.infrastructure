using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Authentication.Domain;
using Authentication.Dtos;
using Infrastructure.CrossCutting.LogService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly ILogService _logService;

        public TokensController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpPost]
        public IActionResult Create([FromBody] TokenCreateDto tokenCreateDto)
        {
            _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Validating credentials | account.Id={tokenCreateDto.Account.Id}");
            if (!CredentialsAreValid(tokenCreateDto.Account))
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Unauthorized | account.Id={tokenCreateDto.Account.Id}");
                return Unauthorized();
            }
            _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | account.Id={tokenCreateDto.Account.Id}");

            // create a claimsIdentity
            var claimsIdentity = new ClaimsIdentity(tokenCreateDto.Claims.Select(c => new Claim(c.Type, c.Value)));

            // create token to the user
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

            // describe security token
            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = UciRodToken.Issuer,
                Subject = claimsIdentity,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(tokenCreateDto.Expire ?? UciRodToken.Expire)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(tokenCreateDto.Account.Secret)), SecurityAlgorithms.HmacSha256Signature)
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

        [Route("validate")]
        [HttpPost]
        public IActionResult Validate([FromBody] TokenValidateDto tokenValidateDto)
        {
            _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Validating credentials | account.Id={tokenValidateDto.Account.Id}");
            if (!CredentialsAreValid(tokenValidateDto.Account))
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Unauthorized | account.Id={tokenValidateDto.Account.Id}");
                return Unauthorized();
            }
            _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | account.Id={tokenValidateDto.Account.Id}");

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = UciRodToken.Issuer,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                LifetimeValidator = LifetimeValidator,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(tokenValidateDto.Account.Secret)),
                ValidateAudience = false
            };

            var identity = tokenHandler.ValidateToken(tokenValidateDto.Token, validationParameters, out var validatedToken);
            _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | JWT security token validated successfully | account.Id={tokenValidateDto.Account.Id}");

            var internalClaimTypes = new[] {"nbf", "exp", "iat", "iss"};

            var claims = identity.Claims.Where(c => !internalClaimTypes.Contains(c.Type));
            _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | account.Id={tokenValidateDto.Account.Id} - claims.Count={claims.Count()}");

            return Ok(new
            {
                Claims = claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
            });
        }

        private static bool CredentialsAreValid(Account account)
        {
            return account.Id == "InventApp" && account.Secret.Equals("1nfr4structur3_1nv3nt4pp");
        }

        private bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (expires == null) return false;

            return DateTime.UtcNow < expires;
        }
    }
}
