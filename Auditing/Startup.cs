using Auditing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.CrossCutting.AppSettings;
using Shared.WebApi;

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

            var sp = services.BuildServiceProvider();
            var appSettingsService = sp.GetService<IAppSettingsService>();

            services.AddDbContext<AuditingDbContext>(options => options.UseSqlServer(appSettingsService.AuditingConnectionString));
        }
    }
}
