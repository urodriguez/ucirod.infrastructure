using jsreport.AspNetCore;
using jsreport.Binary;
using jsreport.Local;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.CrossCuttingV3.AppSettings;
using Shared.WebApiV3;

namespace Rendering
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {

        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            var appSettingsService = services.BuildServiceProvider().GetService<IAppSettingsService>();

            services.AddJsReport(new LocalReporting().Configure(cfg => {
                cfg.HttpPort = appSettingsService.Environment.IsLocal() ? 5489 : 5488;
                return cfg;
            }).UseBinary(JsReportBinary.GetBinary()).AsUtility().Create());
        }
    }
}