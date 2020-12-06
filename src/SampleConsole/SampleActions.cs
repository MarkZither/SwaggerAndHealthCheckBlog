using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationConsole
{
    public class POProcessor : IPOProcessor
    {
        private readonly ILogger _logger;
        public POProcessor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(POProcessor));
        }
        public Task<int> UpdateDatabaseFromPOFiles()
        {
            _logger.LogDebug(1000, "Got {translations} translations from the database ", 2);
            return Task.FromResult(0);
        }
    }

    public interface IPOProcessor
    {
        public Task<int> UpdateDatabaseFromPOFiles();
    }
}
