using System;
using System.Reflection;
using Logging.Application;
using Logging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Exceptions;
using ILogService = Logging.Application.ILogService;

namespace Logging.Controllers
{
    [Route("api/[controller]")]
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
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {_logService.GetCorrelationId()}"
                );
            }
            catch (CorrelationException ce)
            {
                var correlationId = $"CORR_ID_ERROR_{Guid.NewGuid()}";
                var msgToLog = $"CorrelationId = {correlationId} | {ce}";
                //TODO: log locally (example: local file, iis) 'msgToLog'

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {correlationId}"
                );
            }
            catch (LoggingDbException ldbe)
            {
                //Do not call LogService to log this exception in order to avoid infinite loop

                var correlationId = $"LOGGING_DB_ERROR_{Guid.NewGuid()}";
                var msgToLog = $"CorrelationId = {correlationId} | {ldbe}";
                //TODO: log locally (example: local file, iis) 'msgToLog'

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {correlationId}"
                );
            }            
            catch (Exception e)
            {
                _logService.InternalLogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Exception | e.Message={e.Message} - e.StackTrace={e}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An Internal Server Error has ocurred. Please contact with your administrator. CorrelationId = {_logService.GetCorrelationId()}"
                );
            }
        }
    }
}