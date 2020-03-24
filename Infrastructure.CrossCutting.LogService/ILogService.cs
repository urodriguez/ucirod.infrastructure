namespace Logging.Application
{
    public interface ILogService
    {
        void LogTraceMessage(string messageToLog);
        void LogInfoMessage(string messageToLog);
        void LogErrorMessage(string messageToLog);
    }
}