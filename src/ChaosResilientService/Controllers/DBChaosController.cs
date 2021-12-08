using ChaosResilientService.Data;
using ChaosResilientService.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Contrib.Simmy;
using Polly.Contrib.Simmy.Outcomes;
using Polly.Wrap;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChaosResilientService.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class DBChaosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppChaosSettings chaosSettings;
        private readonly ILogger<DBChaosController> _logger;
        private int retries;

        public DBChaosController(ApplicationDbContext context, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, ILogger<DBChaosController> logger)
        {
            _context = context;
            chaosSettings = chaosOptionsSnapshot.Value;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetSomeData(CancellationToken cancellationToken)
        {
            Context context = new Context(nameof(GetSomeData)).WithChaosSettings(chaosSettings);

            retries = 0;
            //https://github.com/PawelGerr/EntityFrameworkCore-Demos/blob/master/src/EntityFramework.Demo/Demos/NamedTransactionsDemo.cs
            //catch (InvalidOperationException ex) when ((ex.InnerException?.InnerException is SqlException sqlEx) && sqlEx.Number == 1205)
            var fault = new InvalidOperationException("Simmy injected a deadlockException");
            var chaosPolicy = MonkeyPolicy.InjectExceptionAsync(with =>
                with.Fault(fault)
                    .InjectionRate(0.95)
                    .Enabled()
                );
            var retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(
               3, // Retry 3 times
               attempt => TimeSpan.FromMilliseconds(200), // Wait 200ms between each try.
               (exception, calculatedWaitDuration) => // Capture some info for logging!
                {
                    // This is your new exception handler! 
                    // Tell the user what they've won!
                    _logger.LogWarning(EventIds.DBReadFailure, exception, "hit an exception");
                   retries++;
               });

            AsyncPolicyWrap faultAndRetryWrap = Policy.WrapAsync(retryPolicy, chaosPolicy);

            try
            {
                // Retry the following call according to the policy - 3 times.
                var something = await faultAndRetryWrap.ExecuteAsync<List<Team>>(
                    async (context, cancellationToken) => // The Execute() overload takes a CancellationToken, but it happens the executed code does not honour it.
                            {
                                // This code is executed within the Policy 
                                var corr = context.CorrelationId;
                                // Make a request and get a response

                                // Display the response message on the console
                                _logger.LogDebug("Getting some data: ");
                                return await _context.Teams.ToListAsync();
                            }
                    , context
                    , cancellationToken // The cancellationToken passed in to Execute() enables the policy instance to cancel retries, when the token is signalled.
                );
                return something;
            }
            catch (Exception e)
            {
                _logger.LogError("Request eventually failed with: " + e.Message);
            }
            return BadRequest();
        }
    }
}
