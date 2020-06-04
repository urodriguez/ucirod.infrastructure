using System.Collections.Generic;
using System.Threading.Tasks;
using Logging.Application.Dtos;

namespace Logging.Application
{
    public interface ILogService
    {
        //Log methods called from Controllers
        Task LogAsync(LogDtoPost logDto);
        Task<IEnumerable<LogSearchResponseDto>> SearchAsync(LogSearchRequestDto logSearchRequestDto);
        void DeleteOldLogs();

        //Log methods called from LogService
        string GetInternalCorrelationId();
        void InternalLogTraceMessageAsync(string messageToLog);
        void InternalLogInfoMessageAsync(string messageToLog);
        void InternalLogErrorMessageAsync(string messageToLog);
        void InternalFileSystemLog(string messageToLog);
    }
}