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
        Task DeleteOldLogsAsync();

        //Log methods called from LogService
        string GetInternalCorrelationId();
        Task InternalLogTraceMessageAsync(string messageToLog);
        Task InternalLogInfoMessageAsync(string messageToLog);
        Task InternalLogErrorMessageAsync(string messageToLog);
        void InternalFileSystemLog(string messageToLog);
    }
}