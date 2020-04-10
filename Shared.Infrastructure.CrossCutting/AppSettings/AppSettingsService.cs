using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Shared.Infrastructure.CrossCutting.AppSettings
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly IConfiguration _configuration;

        public AppSettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string AuditingConnectionString
        {
            get
            {
                switch (Environment.Name)
                {
                    case "DEV": return   "Server=localhost;Database=UciRod.Infrastructure.Auditing;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true";
                    case "TEST": return  "Server=localhost;Database=UciRod.Infrastructure.Auditing-Test;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true";
                    case "STAGE": return "Server=localhost;Database=UciRod.Infrastructure.Auditing-Stage;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true";
                    case "PROD": return  "Server=localhost;Database=UciRod.Infrastructure.Auditing;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true";

                    default: throw new ArgumentOutOfRangeException($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Invalid Environment");
                }
            }
        }

        public Credential Credential => new Credential { Id = "Insfrastructure", SecretKey = "1nfr4structur3_1nfr4structur3" };

        public InsfrastructureEnvironment Environment => new InsfrastructureEnvironment
        {
            Name = _configuration.GetValue<string>("Environment")
        };

        public string LoggingConnectionString
        {
            get
            {
                switch (Environment.Name)
                {
                    case "DEV": return   "Server=localhost;Database=UciRod.Infrastructure.Logging123;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true";
                    case "TEST": return  "Server=localhost;Database=UciRod.Infrastructure.Logging-Test;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true";
                    case "STAGE": return "Server=localhost;Database=UciRod.Infrastructure.Logging-Stage;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true";
                    case "PROD": return  "Server=localhost;Database=UciRod.Infrastructure.Logging;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true";

                    default: throw new ArgumentOutOfRangeException($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Invalid Environment");
                }
            }
        }

        public string LoggingUrl
        {
            get
            {
                const string project = "logging";

                switch (Environment.Name)
                {
                    case "DEV":   return $"http://www.ucirod.infrastructure-test.com:40000/{project}/api";
                    case "TEST":  return $"http://www.ucirod.infrastructure-test.com:40000/{project}/api";
                    case "STAGE": return $"http://www.ucirod.infrastructure-stage.com:40000/{project}/api";
                    case "PROD":  return $"http://www.ucirod.infrastructure.com:40000/{project}/api";

                    default: throw new ArgumentOutOfRangeException($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Invalid Environment");
                }
            }
        }
    }
}