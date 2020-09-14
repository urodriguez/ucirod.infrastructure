namespace Shared.Infrastructure.CrossCutting.Authentication
{
    public interface ICredentialService
    {
        bool AreValid(Credential credential);
    }
}