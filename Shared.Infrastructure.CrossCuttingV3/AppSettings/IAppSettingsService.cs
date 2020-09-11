using Shared.Infrastructure.CrossCuttingV3.Authentication;

namespace Shared.Infrastructure.CrossCuttingV3.AppSettings
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
        string TemplatesRenderedDirectory { get; }
        string RenderingDirectory { get; }
    }
}