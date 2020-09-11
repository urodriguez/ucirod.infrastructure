using System;
using Hangfire;
using Hangfire.SqlServer;
using Logging.Application;
using Logging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Infrastructure.CrossCuttingV3.AppSettings;
using Shared.Infrastructure.CrossCuttingV3.Authentication;

namespace Logging
{
    //Do not use BaseStartup (differents services injection)
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddApiVersioning(config =>
            {
                // Specify the default API Version
                config.DefaultApiVersion = new ApiVersion(1, 0);

                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });

            var appSettingsService = new AppSettingsService(Configuration);
            services.AddSingleton<IAppSettingsService>(s => appSettingsService);

            services.AddDbContext<LoggingDbContext>(options => options.UseSqlServer(appSettingsService.LoggingConnectionString));

            services.AddScoped<ICredentialService, CredentialService>();
            services.AddScoped<ILogService, LogService>();

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(appSettingsService.HangfireLoggingConnectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                }));

            // Add the processing server as IHostedService
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogService logService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHangfireDashboard("/hangfire");
            }
            else
            {
                app.UseHsts();
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new LoggingHangfireDashboardAuthorizationFilter() }
                });
            }

            RecurringJob.AddOrUpdate("delete-old-logs", () => logService.DeleteOldLogsAsync(), Cron.Daily);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
