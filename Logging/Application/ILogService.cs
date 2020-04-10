using System;
using System.Collections.Generic;
using Logging.Application.Dtos;

namespace Logging.Application
{
    public interface ILogService
    {
        Guid GetCorrelationId();
        void Log(LogDtoPost logDto);
        IEnumerable<LogDtoGet> Search(LogSearchRequestDto logSearchRequestDto);

        void InternalLogTraceMessage(string messageToLog);
        void InternalLogInfoMessage(string messageToLog);
        void InternalLogErrorMessage(string messageToLog);
    }
}