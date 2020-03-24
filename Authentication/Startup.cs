using System.Collections.Generic;
using Infrastructure.CrossCutting.Authentication;
using Logging.Application;
using Logging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication
{
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var envConnectionString = new Dictionary<string, string>
            {
                { "DEV", "Server=localhost;Database=UciRod.Infrastructure.Logging;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true" },
                { "TEST", "Server=localhost;Database=UciRod.Infrastructure.Logging-Test;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true" },
                { "STAGE", "Server=localhost;Database=UciRod.Infrastructure.Logging-Stage;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true" },
                { "PROD", "Server=localhost;Database=UciRod.Infrastructure.Logging;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true" }
            };
            var connectionString = envConnectionString[Configuration.GetValue<string>("Environment")];
            services.AddDbContext<LoggingDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Singleton);

            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<ICorrelationService, CorrelationService>();
            services.AddSingleton<ILogService, LogService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
