using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Services.Shared.Extensions;
using System;
using System.IO;
using System.Linq;

namespace LoginService
{
    public static class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        public static int Main(string[] args)
        {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                var seed = args.Contains("/seed");
                if (seed)
                {
                    args = args.Except(new[] { "/seed" }).ToArray();
                }

                var host = CreateHostBuilder(args).Build();
                if (!host.ValidateStartupOptions())
                {
                    return 1;
                }
                if (seed)
                {
                    logger.Info("Seeding database...");
                    var config = host.Services.GetRequiredService<IConfiguration>();
                    var connectionString = config.GetConnectionString("DefaultConnection");
                    SeedData.EnsureSeedData(connectionString);
                    logger.Info("Done seeding database.");
                    return 0;
                }

                logger.Info("Starting host...");
                host.Run();
                return 0;
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (OperatingSystem.IsWindows())
                    {
                        webBuilder.UseHttpSys(
                            httpSysOptions =>
                            {
                                httpSysOptions.Http503Verbosity = Microsoft.AspNetCore.Server.HttpSys.Http503VerbosityLevel.Basic;
                                httpSysOptions.Authentication.AllowAnonymous = true;
                                httpSysOptions.Authentication.AutomaticAuthentication = true;
                            }
                            );
                    }
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                    webBuilder.UseIIS();
                    webBuilder.UseIISIntegration();
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    config.AddConfiguration(Configuration);
                    if (args.Length > 0)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .UseWindowsService()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog(); // NLog: Setup NLog for Dependency injection;
    }
}
