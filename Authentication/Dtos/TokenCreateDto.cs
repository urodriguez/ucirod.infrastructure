using System.Collections.Generic;
using Authentication.Domain;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Authentication.Dtos
{
    public class TokenCreateDto
    {
        public TokenCreateDto()
        {
            Claims = new List<UciRodClaim>();//initialization when Claims in not provided (Claims == null)
        }

        public Credential Credential { get; set; }
        public int? Expires { get; set; }
        public IList<UciRodClaim> Claims { get; set; }
    }
}