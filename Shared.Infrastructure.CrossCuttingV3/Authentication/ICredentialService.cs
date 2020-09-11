namespace Shared.Infrastructure.CrossCuttingV3.Authentication
{
    public interface ICredentialService
    {
        bool AreValid(Credential credential);
    }
}