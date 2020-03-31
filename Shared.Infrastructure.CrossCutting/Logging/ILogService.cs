namespace Shared.Infrastructure.CrossCutting.Logging
{
    public interface ILogService
    {
        void LogTraceMessage(string messageToLog);
        void LogInfoMessage(string messageToLog);
        void LogErrorMessage(string messageToLog);
        void UseProject(string project);
    }
}