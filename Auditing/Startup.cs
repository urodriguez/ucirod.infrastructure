using System.Collections.Generic;
using Auditing.Domain;
using Auditing.Infrastructure.Persistence;
using Infrastructure.CrossCutting.LogService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auditing
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var envConnectionString = new Dictionary<string, string>
            {
                { "DEV", "Server=localhost;Database=UciRod.Infrastructure.Auditing;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true" },
                { "TEST", "Server=localhost;Database=UciRod.Infrastructure.Auditing-Test;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true" },
                { "STAGE", "Server=localhost;Database=UciRod.Infrastructure.Auditing-Stage;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true" },
                { "PROD", "Server=localhost;Database=UciRod.Infrastructure.Auditing;User ID=ucirod-infrastructure-user;Password=uc1r0d-1nfr45tructur3-user;Trusted_Connection=True;MultipleActiveResultSets=true" }
            };

            var env = Configuration.GetValue<string>("Environment");

            var connectionString = envConnectionString[env];
            services.AddDbContext<AuditingDbContext>(options => options.UseSqlServer(connectionString));

            services.AddSingleton<ILogService>(s => new LogService("Infrastructure", "Auditing", env));
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
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
