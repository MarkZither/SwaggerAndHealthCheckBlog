using Microsoft.AspNetCore.Mvc;
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
    public class MonitoringController : Controller
    {
        private readonly MonitoringSettings monitoringSettings;
        private readonly AppChaosSettings chaosSettings;
        private readonly ResilientHttpClient client;

        public MonitoringController(ResilientHttpClient client, IOptions<MonitoringSettings> monitoringOptions, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot)
        {
            this.client = client;
            monitoringSettings = monitoringOptions.Value;
            chaosSettings = chaosOptionsSnapshot.Value;
        }

        [HttpGet]
        public async Task<ActionResult<MonitoringResults>> Status()
        {
            Context context = new Context(nameof(Status)).WithChaosSettings(chaosSettings);

            return await client.GetStatus(monitoringSettings, context);
        }

        [HttpGet]
        public async Task<ActionResult<MonitoringResults>> ResponseTime()
        {
            Context context = new Context(nameof(ResponseTime)).WithChaosSettings(chaosSettings);

            return await client.GetResponseReadTimeMs(monitoringSettings, context);
        }
    }
}
