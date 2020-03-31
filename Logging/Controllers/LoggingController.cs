using System;
using System.Reflection;
using Logging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Exceptions;
using ILogService = Logging.Application.ILogService;

namespace Logging.Controllers
{
    [ApiController]
    public abstract class LoggingController : ControllerBase
    {
        protected readonly ILogService _logService;

        protected LoggingController(ILogService logService)
        {
            _logService = logService;
        }

        protected IActionResult Execute<TResult>(Func<TResult> controllerPipeline) where TResult : IActionResult
        {
            try
            {
                var controllerPipelineResult = controllerPipeline.Invoke();

                _logService.InternalLogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Service Execution Succeed");

                return controllerPipelineResult;
            }
            catch (AuthenticationFailException afe)
            {
                _logService.InternalLogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | AuthenticationFailException");
                return Unauthorized();
            }
            catch (UnauthorizedAccessException uae)
            {
                _logService.InternalLogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | UnauthorizedAccessException | e.Message={uae.Message} - e.StackTrace={uae}");
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            catch (ArgumentNullException ide)
            {
                _logService.InternalLogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentNullException | e.Message={ide.Message} - e.StackTrace={ide}");
                return BadRequest(ide.Message);
            }
            catch (ArgumentOutOfRangeException aore)
            {
                _logService.InternalLogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | ArgumentOutOfRangeException | e.Message={aore.Message} - e.StackTrace={aore}");
                return BadRequest(aore.Message);
            }
            catch (InternalServerException ise)
            {
                _logService.InternalLogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | InternalServerException | e.Message={ise.Message} - e.StackTrace={ise}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (LoggingDbException ldbe)
            {
                //Do not log this exception in order to avoid infinite loop
                //TODO: return correlation id code "LOGGING-DB-ERROR_{date}" and log information into file
                return StatusCode(StatusCodes.Status500InternalServerError, $"e.Message={ldbe.Message} - e.StackTrace={ldbe.OriginalStackTrace}");
            }            
            catch (Exception e)
            {
                _logService.InternalLogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Exception | e.Message={e.Message} - e.StackTrace={e}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}