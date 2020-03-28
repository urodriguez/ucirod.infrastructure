namespace Shared.Infrastructure.CrossCutting.Authentication
{
    public class CredentialService : ICredentialService
    {
        public bool AreValid(Credential credential)
        {
            if (credential == null) return false;

            return credential.Id == "InventApp" && credential.SecretKey.Equals("1nfr4structur3_1nv3nt4pp");
        }
    }
}
