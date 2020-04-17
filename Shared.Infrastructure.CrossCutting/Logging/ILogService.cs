namespace Shared.Infrastructure.CrossCutting.Logging
{
    public interface ILogService
    {
        string GetCorrelationId();
        void LogTraceMessage(string messageToLog);
        void LogInfoMessage(string messageToLog);
        void LogErrorMessage(string messageToLog);
        void UseProject(string project);
    }
}