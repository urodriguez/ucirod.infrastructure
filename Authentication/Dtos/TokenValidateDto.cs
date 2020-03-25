using Infrastructure.CrossCutting.Authentication;

namespace Authentication.Dtos
{
    public class TokenValidateDto
    {
        public Account Account { get; set; }
        public string SecurityToken { get; set; }
    }
}