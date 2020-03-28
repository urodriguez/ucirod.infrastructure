using System;
using System.Reflection;
using Core.Application.Exceptions;
using Infrastructure.CrossCutting.Authentication;
using Logging.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.WebApi
{
    [ApiController]
    public class InfrastructureController : ControllerBase
    {
        private readonly IClientService _clientService;
        protected readonly ILogService _logService;

        public InfrastructureController(IClientService clientService, ILogService logService)
        {
            _clientService = clientService;
            _logService = logService;
        }

        protected IActionResult Execute<TResult>(Account account, Func<TResult> controllerPipeline, MediaType mediaType = MediaType.ApplicationJson) where TResult : IActionResult
        {
            try
            {
                if (!_clientService.CredentialsAreValid(account))
                {
                    if (account == null) throw new ArgumentNullException("Credentials not provided");
                    throw new AuthenticationFailException();
                }
                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | account.Id={account.Id}");

                var controllerPipelineResult = controllerPipeline.Invoke();

                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Service Execution Succeed");

                return controllerPipelineResult;
            }
            catch (AuthenticationFailException afe)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | AuthenticationFailException");
                return Unauthorized();
            }
            catch (UnauthorizedAccessException uae)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | UnauthorizedAccessException | e.Message={uae.Message} - e.StackTrace={uae}");
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            catch (EntryNotFoundException enfe)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | EntryNotFoundException | e.Message={enfe.Message} - e.StackTrace={enfe}");
                return NotFound();
            }
            catch (ArgumentNullException ide)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentNullException | e.Message={ide.Message} - e.StackTrace={ide}");
                return BadRequest(ide.Message);
            }
            catch (ArgumentOutOfRangeException aore)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentOutOfRangeException | e.Message={aore.Message} - e.StackTrace={aore}");
                return BadRequest(aore.Message);
            }
            catch (InternalServerException ise)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | InternalServerException | e.Message={ise.Message} - e.StackTrace={ise}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception e)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Exception | e.Message={e.Message} - e.StackTrace={e}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
