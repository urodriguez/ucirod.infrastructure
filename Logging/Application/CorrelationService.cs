using Logging.Application.Dtos;
using Shared.Application.Exceptions;
using Shared.Infrastructure.CrossCutting.Authentication;
using System;
using Logging.Domain;

namespace Logging.Application
{
    public class CorrelationService : ICorrelationService
    {
        private readonly ICredentialService _credentialService;

        public CorrelationService(ICredentialService credentialService)
        {
            _credentialService = credentialService;
        }

        public CorrelationDto Create(Credential credential, bool validateCredential = true)
        {
            if (validateCredential && !_credentialService.AreValid(credential))
            {
                if (credential == null) throw new ArgumentNullException("Credential not provided");
                throw new AuthenticationFailException();
            }

            var correlation = new Correlation(Guid.NewGuid());

            return new CorrelationDto
            {
                Id = correlation.Id
            };
        }
    }
}