using System.Collections.Generic;
using Logging.Application.Dtos;

namespace Logging.Application
{
    public interface ILogService
    {
        void Configure(LogSettings logSettings);
        void Log(LogDtoPost logDto, bool validateCredentials = true);
        IEnumerable<LogDtoGet> Search(LogSearchRequestDto logSearchRequestDto);

        void LogTraceMessage(string messageToLog);
        void LogInfoMessage(string messageToLog);
        void LogErrorMessage(string messageToLog);
    }
}