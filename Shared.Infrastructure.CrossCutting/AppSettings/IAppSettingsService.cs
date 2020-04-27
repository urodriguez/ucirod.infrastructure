using Shared.Infrastructure.CrossCutting.Authentication;

namespace Shared.Infrastructure.CrossCutting.AppSettings
{
    public interface IAppSettingsService
    {
        string AuditingConnectionString { get; }
        Credential Credential { get; }
        InsfrastructureEnvironment Environment { get; }
        string FileSystemLogsDirectory { get; }
        string HangfireLoggingConnectionString { get; }
        string InsfrastructureDirectory { get; }
        string LoggingConnectionString { get; }
        string LoggingApiUrlV1 { get; }
        string LoggingApiUrlV2 { get; }
        string ReportsDirectory { get; }
        string ReportingDirectory { get; }
    }
}