using Infrastructure.CrossCutting.Authentication;
using Logging.Application.Dtos;

namespace Logging.Application
{
    public interface ICorrelationService
    {
        CorrelationDto Create(Account account, bool validateCredentials = true);
    }
}