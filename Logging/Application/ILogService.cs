using System.Collections.Generic;
using Logging.Application.Dtos;

namespace Logging.Application
{
    public interface ILogService
    {
        void Log(LogDtoPost logDto);
        IEnumerable<LogDtoGet> Search(LogSearchRequestDto logSearchRequestDto);

        void InternalLogTraceMessage(string messageToLog);
        void InternalLogInfoMessage(string messageToLog);
        void InternalLogErrorMessage(string messageToLog);
    }
}