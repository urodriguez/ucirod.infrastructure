using Auditing.Infrastructure.CrossCutting;
using Auditing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.CrossCuttingV3.AppSettings;
using Shared.WebApiV3;

namespace Auditing
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {

        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddSingleton<IJsonService, JsonService>();

            var sp = services.BuildServiceProvider();
            var appSettingsService = sp.GetService<IAppSettingsService>();

            services.AddDbContext<AuditingDbContext>(options => options.UseSqlServer(appSettingsService.AuditingConnectionString));
        }
    }
}