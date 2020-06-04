using System;
using System.Reflection;
using System.Threading.Tasks;
using Logging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Exceptions;
using ILogService = Logging.Application.ILogService;

namespace Logging.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public abstract class LoggingController : ControllerBase
    {
        protected readonly ILogService _logService;

        protected LoggingController(ILogService logService)
        {
            _logService = logService;
        }

        protected async Task<IActionResult> ExecuteAsync<TResult>(Func<Task<TResult>> controllerPipeline) where TResult : IActionResult
        {
            try
            {
                var controllerPipelineResult = await controllerPipeline.Invoke();

                _logService.InternalLogInfoMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Service Execution Succeed");

                return controllerPipelineResult;
            }
            catch (AuthenticationFailException afe)
            {
                _logService.InternalLogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | AuthenticationFailException");
                return Unauthorized();
            }
            catch (UnauthorizedAccessException uae)
            {
                _logService.InternalLogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | UnauthorizedAccessException | e.Message={uae.Message} - e.StackTrace={uae}");
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            catch (ArgumentNullException ide)
            {
                _logService.InternalLogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentNullException | e.Message={ide.Message} - e.StackTrace={ide}");
                return BadRequest(ide.Message);
            }
            catch (ArgumentOutOfRangeException aore)
            {
                _logService.InternalLogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentOutOfRangeException | e.Message={aore.Message} - e.StackTrace={aore}");
                return BadRequest(aore.Message);
            }
            catch (InternalServerException ise)
            {
                _logService.InternalLogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | InternalServerException | e.Message={ise.Message} - e.StackTrace={ise}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {_logService.GetInternalCorrelationId()}"
                );
            }
            catch (LoggingDbException ldbe)
            {
                //Do not call LogService to log this exception in order to avoid infinite loop
                _logService.InternalFileSystemLog($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | LoggingDbException | e={ldbe}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {_logService.GetInternalCorrelationId()}"
                );
            }            
            catch (Exception e)
            {
                _logService.InternalLogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Exception | e.Message={e.Message} - e.StackTrace={e}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {_logService.GetInternalCorrelationId()}"
                );
            }
        }
    }
}