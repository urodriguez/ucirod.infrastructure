namespace Authentication
{
    public class TokenValidateDto
    {
        public Account Account { get; set; }
        public string Token { get; set; }
    }
}