using ChaosResilientService.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Polly;
using Polly.Extensions.Http;
using Polly.Registry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChaosResilientService
{
    public class Startup
    {
        public const string ResiliencePolicy = "ResiliencePolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("InMemory"));

            services.AddControllers();

            // Read the endpoints we expect the service to monitor.
            services.Configure<MonitoringSettings>(Configuration.GetSection("MonitoringEndpoints"));

            // Create (and register with DI) a policy registry containing some policies we want to use.
            services.AddPolicyRegistry(new PolicyRegistry
            {
                { ResiliencePolicy, GetResiliencePolicy() }
            });

            // Register a typed client via HttpClientFactory, set to use the policy we placed in the policy registry.
            services.AddHttpClient<ResilientHttpClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            })
                .AddPolicyHandlerFromRegistry(ResiliencePolicy);

            // Add ability for the app to populate ChaosSettings from json file (or any other .NET Core configuration source)
            services.Configure<AppChaosSettings>(Configuration.GetSection("ChaosSettings"));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChaosResilientService", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChaosResilientService v1"));
            }

            // Only add Simmy chaos injection in development-environment runs (ie prevent chaos-injection ever reaching staging or prod - if that is what you want).
            if (env.IsDevelopment())
            {
                // Wrap every policy in the policy registry in Simmy chaos injectors.
                var registry = app.ApplicationServices.GetRequiredService<IPolicyRegistry<string>>();
                registry?.AddChaosInjectors();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy()
        {
            // Define a policy which will form our resilience strategy.  These could be anything.  The settings for them could obviously be drawn from config too.
            var retry = HttpPolicyExtensions.HandleTransientHttpError()
                .RetryAsync(2);

            return retry;
        }
    }
}
