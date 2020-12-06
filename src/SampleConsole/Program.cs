using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Generic;

namespace TranslationConsole
{
    class Program
    {
        static async Task Main(string[] args) => await BuildCommandLine()
                    .UseHost(_ => Host.CreateDefaultBuilder(),
                        host =>
                        {
                            host.ConfigureAppConfiguration((hostingContext, config) =>
                            {
                                // The next 2 lines commented out as they are added by default in the correct order
                                // for more control first call config.Sources.Clear();
                                //config.AddJsonFile("appsettings.json", optional: true);
                                //config.AddEnvironmentVariables();
                                config.AddUserSecrets<Program>();

                                if (args != null)
                                {
                                    var switchMappings = new Dictionary<string, string>()
                                     {
                                         { "-c", "connectionstrings:mydb" },
                                         { "--connectionString", "connectionstrings:mydb" },
                                     };
                                    config.AddCommandLine(args, switchMappings);
                                }
                            })
                            .ConfigureServices((hostContext, services) =>
                            {
                                services.AddSingleton<IPOProcessor, POProcessor>();
                            });
                        })
                    .UseDefaults()
                    .Build()
                    .InvokeAsync(args);

        private static CommandLineBuilder BuildCommandLine()
        {
            var root = new RootCommand(@"$ TranslationConsole --translationDirectory ""c:\Translations\sample1"" --usedIn 1 -c ""Data Source=.\MSSQLSERVER2017;Database=MyDb;Trusted_Connection=True;"""){
                new Option<string>(aliases: new string[] { "--usedIn", "-u" }){
                    Description = "The id of selected system(sample1 - 1; sample2 - 2; sample3 - 3; sample4 - 4). If value is not passed all systems will be updated.",
                    IsRequired = false
                }, new Option<string>(aliases: new string[] { "--translationDirectory", "-td" }){
                    Description = "The path to the root directory containing all translation directories",
                    IsRequired = true
                },
                new Option<string>(aliases: new string[] { "--connectionString", "-c" }){
                    Description = "The database connection string e.g. 'Data Source=.\\MSSQLSERVER2017;Database=MyDB;Trusted_Connection=True;'",
                    IsRequired = true
                },
            };
            root.Handler = CommandHandler.Create<TranslationOptions, IHost>(Run);
            return new CommandLineBuilder(root);
        }

        private static void Run(TranslationOptions options, IHost host)
        {
            var serviceProvider = host.Services;
            var poProcessor = serviceProvider.GetRequiredService<IPOProcessor>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(Program));

            var translationDirectory = options.TranslationDirectory;
            logger.LogInformation(1000, "Update requested for: {translationDirectory}", translationDirectory);
            poProcessor.UpdateDatabaseFromPOFiles();
        }
    }
}
