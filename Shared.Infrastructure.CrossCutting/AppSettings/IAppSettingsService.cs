using Shared.Infrastructure.CrossCutting.Authentication;

namespace Shared.Infrastructure.CrossCutting.AppSettings
{
    public interface IAppSettingsService
    {
        string AuditingConnectionString { get; }
        Credential Credential { get; }
        InsfrastructureEnvironment Environment { get; }
        string LoggingConnectionString { get; }
        string LoggingUrl { get; }
    }
}