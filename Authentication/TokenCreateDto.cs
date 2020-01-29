using System.Collections.Generic;

namespace Authentication
{
    public class TokenCreateDto
    {
        public Account Account { get; set; }
        public int? Expire { get; set; }
        public IList<UciRodClaim> Claims { get; set; }
    }
}