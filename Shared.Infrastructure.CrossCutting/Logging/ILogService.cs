namespace Shared.Infrastructure.CrossCutting.Logging
{
    public interface ILogService
    {
        string GetCorrelationId();
        void LogTraceMessageAsync(string messageToLog);
        void LogInfoMessageAsync(string messageToLog);
        void LogErrorMessageAsync(string messageToLog);
        void UseProject(string project);
    }
}