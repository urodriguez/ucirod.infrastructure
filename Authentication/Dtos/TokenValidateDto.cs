using Shared.Infrastructure.CrossCuttingV3.Authentication;

namespace Authentication.Dtos
{
    public class TokenValidateDto
    {
        public Credential Credential { get; set; }
        public string SecurityToken { get; set; }
    }
}