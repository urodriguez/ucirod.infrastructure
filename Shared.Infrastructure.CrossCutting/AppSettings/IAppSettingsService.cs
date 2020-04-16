using System.Data.Common;
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
        string LoggingUrl { get; }
    }
}