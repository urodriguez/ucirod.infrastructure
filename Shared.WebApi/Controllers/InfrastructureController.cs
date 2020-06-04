using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Exceptions;
using Shared.Infrastructure.CrossCutting.Authentication;
using Shared.Infrastructure.CrossCutting.Logging;

namespace Shared.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public abstract class InfrastructureController : ControllerBase
    {
        private readonly ICredentialService _credentialService;
        protected readonly ILogService _logService;

        protected InfrastructureController(ICredentialService credentialService, ILogService logService)
        {
            _credentialService = credentialService;
            _logService = logService;

            ConfigureLogging();
        }

        protected IActionResult Execute<TResult>(Credential credential, Func<TResult> controllerPipeline, MediaType mediaType = MediaType.ApplicationJson) where TResult : IActionResult
        {
            try
            {
                if (!_credentialService.AreValid(credential))
                {
                    if (credential == null) throw new ArgumentNullException("Credential not provided");
                    throw new AuthenticationFailException();
                }

                var controllerPipelineResult = controllerPipeline.Invoke();

                _logService.LogInfoMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Service Execution Succeed");

                return controllerPipelineResult;
            }
            catch (AuthenticationFailException afe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | AuthenticationFailException");
                return Unauthorized();
            }
            catch (UnauthorizedAccessException uae)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | UnauthorizedAccessException | e.Message={uae.Message} - e.StackTrace={uae}");
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            catch (EntryNotFoundException enfe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | EntryNotFoundException | e.Message={enfe.Message} - e.StackTrace={enfe}");
                return NotFound();
            }
            catch (ArgumentNullException ide)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentNullException | e.Message={ide.Message} - e.StackTrace={ide}");
                return BadRequest(ide.Message);
            }
            catch (ArgumentOutOfRangeException aore)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentOutOfRangeException | e.Message={aore.Message} - e.StackTrace={aore}");
                return BadRequest(aore.Message);
            }            
            catch (InvalidOperationException ioe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | InvalidOperationException | e.Message={ioe.Message} - e.StackTrace={ioe}");
                return BadRequest(ioe.Message);
            }            
            catch (KeyNotFoundException knfe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | KeyNotFoundException | e.Message={knfe.Message} - e.StackTrace={knfe}");
                return BadRequest(knfe.Message);
            }            
            catch (FormatException fe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | FormatException | e.Message={fe.Message} - e.StackTrace={fe}");
                return BadRequest(fe.Message);
            }
            catch (InternalServerException ise)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | InternalServerException | e={ise}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {_logService.GetCorrelationId()}"
                );
            }
            catch (Exception e)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Exception | e={e}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {_logService.GetCorrelationId()}"
                );
            }
        }

        protected async Task<IActionResult> ExecuteAsync<TResult>(Credential credential, Func<Task<TResult>> controllerPipeline, MediaType mediaType = MediaType.ApplicationJson) where TResult : IActionResult
        {
            try
            {
                if (!_credentialService.AreValid(credential))
                {
                    if (credential == null) throw new ArgumentNullException("Credential not provided");
                    throw new AuthenticationFailException();
                }

                var controllerPipelineResult = await controllerPipeline.Invoke();

                _logService.LogInfoMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Service Execution Succeed");

                return controllerPipelineResult;
            }
            catch (AuthenticationFailException afe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | AuthenticationFailException");
                return Unauthorized();
            }
            catch (UnauthorizedAccessException uae)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | UnauthorizedAccessException | e.Message={uae.Message} - e.StackTrace={uae}");
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            catch (EntryNotFoundException enfe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | EntryNotFoundException | e.Message={enfe.Message} - e.StackTrace={enfe}");
                return NotFound();
            }
            catch (ArgumentNullException ide)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentNullException | e.Message={ide.Message} - e.StackTrace={ide}");
                return BadRequest(ide.Message);
            }
            catch (ArgumentOutOfRangeException aore)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentOutOfRangeException | e.Message={aore.Message} - e.StackTrace={aore}");
                return BadRequest(aore.Message);
            }
            catch (InvalidOperationException ioe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | InvalidOperationException | e.Message={ioe.Message} - e.StackTrace={ioe}");
                return BadRequest(ioe.Message);
            }
            catch (KeyNotFoundException knfe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | KeyNotFoundException | e.Message={knfe.Message} - e.StackTrace={knfe}");
                return BadRequest(knfe.Message);
            }
            catch (FormatException fe)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | FormatException | e.Message={fe.Message} - e.StackTrace={fe}");
                return BadRequest(fe.Message);
            }
            catch (InternalServerException ise)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | InternalServerException | e={ise}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {_logService.GetCorrelationId()}"
                );
            }
            catch (Exception e)
            {
                _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Exception | e={e}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {_logService.GetCorrelationId()}"
                );
            }
        }

        protected abstract void ConfigureLogging();
    }
}
