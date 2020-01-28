using System.Collections.Generic;
using System.Security.Claims;

namespace Authentication
{
    public class TokenCreateDto
    {
        public Account Account { get; set; }
        public IList<UciRodClaim> Claims { get; set; }
    }
}