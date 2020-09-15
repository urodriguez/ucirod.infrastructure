using System.Collections.Generic;
using System.Linq;

namespace Shared.Infrastructure.CrossCutting.Authentication
{
    public class CredentialService : ICredentialService
    {
        private readonly IList<Credential> _validCredentials = new List<Credential>
        {
            new Credential
            {
                Id = "InventApp",
                SecretKey = "1nfr4structur3_1nv3nt4pp"
            },            
            new Credential
            {
                Id = "Insfrastructure",
                SecretKey = "1nfr4structur3_1nfr4structur3"
            },
            new Credential
            {
                Id = "Choby",
                SecretKey = "1nfr4structur3_ch0by"
            }
        };

        public bool AreValid(Credential credential)
        {
            if (credential == null) return false;

            return _validCredentials.Any(vc => vc.Id == credential.Id && vc.SecretKey == credential.SecretKey);
        }
    }
}
