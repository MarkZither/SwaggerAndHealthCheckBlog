using Finbuckle.MultiTenant;

using Flurl;

using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ResourceService.Configuration;
using ResourceService.DataAccess;
using ResourceService.Extensions;
using ResourceService.Middlewares;

using Services.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace ResourceService
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
            services.AddControllers();

            var resourceOptions = new ResourceOptions();
            Configuration.Bind(resourceOptions);

            services.AddOptions<ResourceOptions>()
                .Bind(Configuration.GetSection(ResourceOptions.Resource))
                .ValidateDataAnnotations();

            services.AddOptions();

            services.AddResourceServices();

            services.AddAuthentication("Bearer")
                /*.AddJwtBearer("Bearer", options =>
                {
                    options.Audience = "api1";
                    options.Authority = resourceOptions.IdentityServerUrl;
                })*/
                // https://www.scottbrady91.com/identity-server/aspnet-core-swagger-ui-authorization-using-identityserver4
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    // required audience of access tokens
                    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                    // as we use options.EmitStaticAudienceClaim = true; in loginservice startup we use this ApiName
                    options.ApiName = resourceOptions.IdentityServerUrl.AppendPathSegment("resources"); //"api1";

                    // auth server base endpoint (this will be used to search for disco doc)
                    options.Authority = resourceOptions.IdentityServerUrl;
                });

            var targetHost = "www.microsoft.com";
            var targetHostIpAddresses = Dns.GetHostAddresses(targetHost).Select(h => h.ToString()).ToArray();

            var targetHost2 = "localhost";
            var targetHost2IpAddresses = Dns.GetHostAddresses(targetHost2).Select(h => h.ToString()).ToArray();
            var maximumMemory = 104857600;

            services.AddHealthChecks()
                .AddDbContextCheck<ResourceDataContext>()
                .AddIdentityServer(new Uri(resourceOptions.IdentityServerUrl), "test IdSrv", tags: new string[] { "IdSrv" })
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
                    setup.AddHost("127.0.0.1", 1121);
                }, tags: new string[] { "tcp" }, name: "Logging TCP port Check")
                .AddPrivateMemoryHealthCheck(maximumMemory
                , tags: new string[] { "privatememory" }, name: "PrivateMemory Check")
                .AddWorkingSetHealthCheck(maximumMemory
                , tags: new string[] { "workingset" }, name: "WorkingSet Check")
                .AddVirtualMemorySizeHealthCheck(maximumMemory
                , tags: new string[] { "virtualmemory" }, name: "VirtualMemory Check");
            // Add a health check for a SQL Server database
            services.AddDbContext<ResourceDataContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("ResourceDb")));
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger and HealthCheck blog Login Service", Version = "v1" });
                c.OperationFilter<AuthorizeOperationFilter>();
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://localhost:1115/connect/authorize"),
                            TokenUrl = new Uri("https://localhost:1115/connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                {"api1", "Demo API - full access"}
                            }
                        }
                    }
                });
            });

            services.AddSingleton<IAuthorizationMiddlewareResultHandler,
                                    MyAuthorizationMiddlewareResultHandler>();

            // Multi Tenant Services
            services.AddMultiTenant<TenantInfo>()
            .WithRouteStrategy()
            .WithConfigurationStore();
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
                c.OAuthClientId("demo_api_swagger");
                c.OAuthUsePkce();
            });

            app.UseMultiTenant();   // Before UseAuthentication and UseMvc!!
                                    //https://www.finbuckle.com/MultiTenant/Docs/ConfigurationAndUsage#usemultitenant
                                    // https://www.finbuckle.com/MultiTenant/Docs/GettingStarted

            app.UseAuthentication();
            app.UseAuthorization();

            // HealthCheck middleware
            app.UseHealthChecks("/hc", $"{Configuration["ManagementPort"]}", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{__tenant__=}/{controller=Home}/{action=Index}");
                endpoints.MapHealthChecks("/health").RequireHost($"*:{Configuration["ManagementPort"]}");
            });
        }
    }
}
