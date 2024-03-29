using HealthChecks.UI.Client;
using LoginService.Configuration;
using LoginService.Data;
using LoginService.DataAccess;
using LoginService.Extensions;
using LoginService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Services.Shared.Extensions;
using App.Metrics;
using App.Metrics.Formatters.Json;
using Microsoft.AspNetCore.Mvc;
using System;
using App.Metrics.Filtering;

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
            var filter = new MetricsFilter().WhereType(MetricType.Timer);
            var metrics = new MetricsBuilder()
                /*.Report.ToConsole(
                    options =>
                    {
                        options.FlushInterval = TimeSpan.FromSeconds(5);
                        options.Filter = filter;
                        options.MetricsOutputFormatter = new MetricsJsonOutputFormatter();
                    })*/
                .Report.ToHostedMetrics(
                "https://graphite-us-central1.grafana.net/metrics",
                //"42196:eyJrIjoiNzU1OTYwOWU0ZjAyYzlhOWYyYzE1YzM3MDg2ZTEwYWI4YWU4OTUyYiIsIm4iOiJNZXRyaWNzUHVibGlzaGVyIiwiaWQiOjQ2NDIwNn0="
                "42196:eyJrIjoiZTJiNjJjNGYwYjRhMTYwZTM2MTEwZDJlMGEzNTIwNWQ5MWQ1YjQ2MCIsIm4iOiJDbG91ZEdyYXBoaXRlIiwiaWQiOjQ2NDIwNn0="
                )
                .Build();


            services.AddMvc().AddNewtonsoftJson().AddMetrics();

            //services.AddAppMetricsSystemMetricsCollector();
            //services.AddAppMetricsGcEventsMetricsCollector();

            services.AddLoginServices();
            services.AddLoginServiceHealthChecks(Configuration);

            services.AddOptions<LoginOptions>()
            .Bind(Configuration.GetSection(LoginOptions.Login))
            .ValidateDataAnnotations();

            // Add a health check for a SQL Server database
            services.AddDbContext<AuthContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("LoginServiceDb")));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>();

            services.AddLocalApiAuthentication();

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = "https://localhost:1116";
                    options.Audience = "https://localhost:1116/resources";
                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                });

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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSwaggerUrlPortAuthMiddleware();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Login Service API V1");
            });

            app.UseIdentityServer();
            app.UseAuthorization();

            // HealthCheck middleware
            app.UseHealthChecks("/hc", $"{Configuration["ManagementPort"]}", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapHealthChecks("/health").RequireHost($"*:{Configuration["ManagementPort"]}");
            });
        }
    }
}
