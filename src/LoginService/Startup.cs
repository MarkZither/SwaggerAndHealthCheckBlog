using HealthChecks.UI.Client;
using LoginService.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Linq;
using System.Net;

using Services.Shared.Extensions;

namespace LoginService
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
            services.AddMvc().AddNewtonsoftJson();

            var targetHost = "www.microsoft.com";
            var targetHostIpAddresses = Dns.GetHostAddresses(targetHost).Select(h => h.ToString()).ToArray();

            var targetHost2 = "localhost";
            var targetHost2IpAddresses = Dns.GetHostAddresses(targetHost2).Select(h => h.ToString()).ToArray();
            var maximumMemory = 104857600;

            services.AddHealthChecks()
                .AddDnsResolveHealthCheck(setup =>
                {
                    setup.ResolveHost(targetHost).To(targetHostIpAddresses)
                    .ResolveHost(targetHost2).To(targetHost2IpAddresses);
                }, tags: new string[] { "dns" }, name: "DNS Check")
                .AddPingHealthCheck(setup =>
                {
                    setup.AddHost("127.0.0.1", 5000);
                }, tags: new string[] { "ping" }, name: "Ping Check")
                .AddTcpHealthCheck(setup =>
                {
                    setup.AddHost("127.0.0.1", 1116);
                }, tags: new string[] { "tcp" }, name: "TCP port Check")
                .AddPrivateMemoryHealthCheck(maximumMemory
                , tags: new string[] { "privatememory" }, name: "PrivateMemory Check")
                .AddWorkingSetHealthCheck(maximumMemory
                , tags: new string[] { "workingset" }, name: "WorkingSet Check")
                .AddVirtualMemorySizeHealthCheck(maximumMemory
                , tags: new string[] { "virtualmemory" }, name: "VirtualMemory Check");
            // Add a health check for a SQL Server database
            services.AddDbContext<AuthContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("LoginServiceDb")));

            services.AddOptions();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger and HealthCheck blog Login Service", Version = "v1" });
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSwaggerUrlPortAuthMiddleware();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Login Service API V1");
            });

            app.UseAuthorization();

            // HealthCheck middleware
            app.UseHealthChecks("/hc", $"{Configuration["ManagementPort"]}", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health").RequireHost($"*:{Configuration["ManagementPort"]}");
            });
        }
    }
}
