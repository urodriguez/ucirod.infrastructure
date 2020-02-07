using System.Collections.Generic;
using Authentication.Domain;

namespace Authentication.Dtos
{
    public class TokenCreateDto
    {
        public Account Account { get; set; }
        public int? Expire { get; set; }
        public IList<UciRodClaim> Claims { get; set; }
    }
}