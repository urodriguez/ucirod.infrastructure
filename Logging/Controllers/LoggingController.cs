using System;
using System.Reflection;
using Core.Application.Exceptions;
using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Logging.Controllers
{
    [ApiController]
    public class LoggingController : ControllerBase
    {
        protected readonly ILogService _logService;

        public LoggingController(ILogService logService, ICorrelationService correlationService, IConfiguration config)
        {
            _logService = logService;

            _logService.Configure(new LogSettings
            {
                Application = "Infrastructure",
                Project = "Logging",
                Environment = config.GetValue<string>("Environment"),
                CorrelationId = correlationService.Create(null, false).Id
            });
        }

        protected IActionResult Execute<TResult>(Func<TResult> controllerPipeline) where TResult : IActionResult
        {
            try
            {
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