using HealthChecks.Network.Core;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Services.Shared.Extensions;
using System.Linq;
using System.Net;

namespace NotificationService
{
    public class Startup
    {
        private readonly string _host = "smtp.gmail.com";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

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
                .AddSmtpHealthCheck(setup =>
                {
                    //SSL on by default
                    setup.Host = _host;
                    setup.Port = 465;
                    setup.ConnectionType = SmtpConnectionType.SSL;
                    setup.AllowInvalidRemoteCertificates = true;
                }, tags: new string[] { "smtp" })
                .AddPrivateMemoryHealthCheck(maximumMemory
                , tags: new string[] { "privatememory" }, name: "PrivateMemory Check")
                .AddWorkingSetHealthCheck(maximumMemory
                , tags: new string[] { "workingset" }, name: "WorkingSet Check")
                .AddVirtualMemorySizeHealthCheck(maximumMemory
                , tags: new string[] { "virtualmemory" }, name: "VirtualMemory Check");

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger and HealthCheck blog Notification Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

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
