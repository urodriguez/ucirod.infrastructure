using Authentication.Domain;

namespace Authentication.Dtos
{
    public class TokenValidateDto
    {
        public Account Account { get; set; }
        public TokenStatus TokenStatus { get; set; }
        public string Token { get; set; }

    }
}