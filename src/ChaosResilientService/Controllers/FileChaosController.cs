using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChaosResilientService.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class FileChaosController : Controller
    {
        private readonly string FilePolicy;
        private readonly MonitoringSettings monitoringSettings;
        private readonly AppChaosSettings chaosSettings;
        private readonly ResilientHttpClient client;
        private readonly ILogger<FileChaosController> _logger;

        public FileChaosController(ResilientHttpClient client,
                                   IOptions<MonitoringSettings> monitoringOptions,
                                   IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot,
                                   ILogger<FileChaosController> logger)
        {
            this.client = client;
            monitoringSettings = monitoringOptions.Value;
            chaosSettings = chaosOptionsSnapshot.Value;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<MonitoringResults>> Get()
        {
            Context context = new Context(nameof(FilePolicy)).WithChaosSettings(chaosSettings);

            string text = System.IO.File.ReadAllText(@"TestFile.txt");

            return Ok();
        }
    }
}
