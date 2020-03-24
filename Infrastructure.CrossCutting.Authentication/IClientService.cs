namespace Infrastructure.CrossCutting.Authentication
{
    public interface IClientService
    {
        bool CredentialsAreValid(Account account);
    }
}