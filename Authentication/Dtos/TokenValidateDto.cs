using Shared.Infrastructure.CrossCutting.Authentication;

namespace Authentication.Dtos
{
    public class TokenValidateDto
    {
        public Credential Credential { get; set; }
        public string SecurityToken { get; set; }
    }
}