using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Shared.Infrastructure.CrossCutting.AppSettings
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly string _baseInfrastructureApiUrl;
        private readonly IConfiguration _configuration;

        public AppSettingsService(IConfiguration configuration)
        {
            _configuration = configuration;

            const int infrastructureApiPort = 8081;

            const string sqlServerAuditingDatabase = "UciRod.Infrastructure.Auditing";
            const string sqlServerLoggingDatabase = "UciRod.Infrastructure.Logging";
            string sqlServerLoggingHangfireDatabase = $"{sqlServerLoggingDatabase}.Hangfire";

            const string sqlServerUser = "ucirod-infrastructure-user";
            const string sqlServerPassword = "uc1r0d-1nfr45tructur3-user";

            const string multipleActiveResultSetsTrue = "MultipleActiveResultSets=True";
            const string integratedSecuritySspi = "Integrated Security=SSPI";

            InsfrastructureDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @".."));

            switch (Environment.Name)
            {
                case "DEV":
                {
                    const string sqlServerInstance = "localhost";
                    AuditingConnectionString = $"Server={sqlServerInstance};Database={sqlServerAuditingDatabase};User ID={sqlServerUser};Password={sqlServerPassword};{multipleActiveResultSetsTrue}";
                    _baseInfrastructureApiUrl = $"www.ucirod.infrastructure-test.com:{infrastructureApiPort}";
                    HangfireLoggingConnectionString = $"Server={sqlServerInstance};Database={sqlServerLoggingHangfireDatabase};{integratedSecuritySspi}";
                    LoggingConnectionString = $"Server={sqlServerInstance};Database={sqlServerLoggingDatabase};User ID={sqlServerUser};Password={sqlServerPassword};{multipleActiveResultSetsTrue}";
                    InsfrastructureDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")), @".."));
                    
                    break;
                }                
                
                case "TEST":
                {
                    const string sqlServerInstance = "localhost";
                    AuditingConnectionString = $"Server={sqlServerInstance};Database={sqlServerAuditingDatabase}-Test;User ID={sqlServerUser};Password={sqlServerPassword};{multipleActiveResultSetsTrue}";
                    _baseInfrastructureApiUrl = $"www.ucirod.infrastructure-test.com:{infrastructureApiPort}";
                    HangfireLoggingConnectionString = $"Server={sqlServerInstance};Database={sqlServerLoggingHangfireDatabase}-Test;{integratedSecuritySspi}";
                    LoggingConnectionString = $"Server={sqlServerInstance};Database={sqlServerLoggingDatabase}-Test;User ID={sqlServerUser};Password={sqlServerPassword};{multipleActiveResultSetsTrue}";
                    
                    break;
                }                
                
                case "STAGE":
                {
                    const string sqlServerInstance = "ucirod-stage1234.amazonaws.com";
                    AuditingConnectionString = $"Server={sqlServerInstance};Database={sqlServerAuditingDatabase};User ID={sqlServerUser};Password={sqlServerPassword};{multipleActiveResultSetsTrue}";
                    _baseInfrastructureApiUrl = $"www.ucirod.infrastructure-stage.com:{infrastructureApiPort}";
                    HangfireLoggingConnectionString = $"Server={sqlServerInstance};Database={sqlServerLoggingHangfireDatabase};{integratedSecuritySspi}";
                    LoggingConnectionString = $"Server={sqlServerInstance};Database={sqlServerLoggingDatabase};User ID={sqlServerUser};Password={sqlServerPassword};{multipleActiveResultSetsTrue}";
                   
                    break;
                }                
                
                case "PROD":
                {
                    const string sqlServerInstance = "ucirod-prod1234.amazonaws.com";
                    AuditingConnectionString = $"Server={sqlServerInstance};Database={sqlServerAuditingDatabase};User ID={sqlServerUser};Password={sqlServerPassword};{multipleActiveResultSetsTrue}";
                    _baseInfrastructureApiUrl = $"www.ucirod.infrastructure.com:{infrastructureApiPort}";
                    HangfireLoggingConnectionString = $"Server={sqlServerInstance};Database={sqlServerLoggingHangfireDatabase};{integratedSecuritySspi}";
                    LoggingConnectionString = $"Server={sqlServerInstance};Database={sqlServerLoggingDatabase};User ID={sqlServerUser};Password={sqlServerPassword};{multipleActiveResultSetsTrue}";
                    
                    break;
                }

                default: throw new ArgumentOutOfRangeException($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Invalid Environment");
            }
        }

        public string AuditingConnectionString { get; }

        public Credential Credential => new Credential { Id = "Insfrastructure", SecretKey = "1nfr4structur3_1nfr4structur3" };

        public InsfrastructureEnvironment Environment => new InsfrastructureEnvironment { Name = _configuration.GetValue<string>("Environment") };

        public string FileSystemLogsDirectory => $"{InsfrastructureDirectory}\\FileSystemLogs";

        public string HangfireLoggingConnectionString { get; }

        public string InsfrastructureDirectory { get; }

        public string LoggingConnectionString { get; }

        public string LoggingApiUrlV1 => $"http://{_baseInfrastructureApiUrl}/logging/api/v1.0";

        public string LoggingApiUrlV2 => LoggingApiUrlV1.Replace("v1.0", "v2.0");

        public string ReportsDirectory => $"{ReportingDirectory}\\Reports";

        public string ReportingDirectory => $"{InsfrastructureDirectory}\\Reporting";
    }
}