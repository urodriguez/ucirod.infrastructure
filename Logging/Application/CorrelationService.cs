using System;
using System.Reflection;
using Infrastructure.CrossCutting.Authentication;
using Logging.Application.Dtos;

namespace Logging.Application
{
    public class CorrelationService : ICorrelationService
    {
        private readonly IClientService _clientService;
        private readonly ILogService _logService;

        public CorrelationService(IClientService clientService, ILogService logService)
        {
            _clientService = clientService;
            _logService = logService;
        }

        public CorrelationDto Create(Account account, bool validateCredentials = true)
        {
            if (validateCredentials)
            {
                if (!_clientService.CredentialsAreValid(account))
                {
                    if (account == null)
                    {
                        _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | BadRequest | account == null");
                        throw new ArgumentNullException("Credentials not provided");
                    }
                    _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Unauthorized | account.Id={account.Id}");
                    throw new UnauthorizedAccessException();
                }
                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | account.Id={account.Id}");
            }

            return new CorrelationDto();
        }
    }
}