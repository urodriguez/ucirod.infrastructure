namespace Infrastructure.CrossCutting.Authentication
{
    public class ClientService : IClientService
    {
        public bool CredentialsAreValid(Account account)
        {
            if (account == null) return false;

            return account.Id == "InventApp" && account.SecretKey.Equals("1nfr4structur3_1nv3nt4pp");
        }
    }
}
