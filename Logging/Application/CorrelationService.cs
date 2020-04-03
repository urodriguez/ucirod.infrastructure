using System;
using System.Reflection;
using Logging.Application.Dtos;
using Shared.Application.Exceptions;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Logging.Application
{
    public class CorrelationService : ICorrelationService
    {
        private readonly ICredentialService _credentialService;
        private readonly ILogService _logService;

        public CorrelationService(ICredentialService credentialService, ILogService logService)
        {
            _credentialService = credentialService;
            _logService = logService;
        }

        public CorrelationDto Create(Credential credential)
        {
            if (!_credentialService.AreValid(credential))
            {
                if (credential == null) throw new ArgumentNullException("Credential not provided");
                throw new AuthenticationFailException();
            }
            _logService.InternalLogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | credential.Id={credential.Id}");

            return new CorrelationDto();
        }
    }
}