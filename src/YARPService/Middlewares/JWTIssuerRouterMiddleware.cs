using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ReverseProxy.Middleware;
using Microsoft.ReverseProxy.RuntimeModel;
using Microsoft.ReverseProxy.Service.Proxy;

namespace YARPService.Middlewares
{
    /// <summary>
    /// Load balances across the available destinations.
    /// </summary>
    public class JWTIssuerRouterMiddleware {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public JWTIssuerRouterMiddleware(
            RequestDelegate next,
            ILogger<JWTIssuerRouterMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Invoke(HttpContext context)
        {
            _logger.LogWarning(EventIds.TestWarning, "Test Test");
            var proxyFeature = context.Features.Get<IReverseProxyFeature>();
            var destinations = proxyFeature.AvailableDestinations;
            var jwt = context.Request.Headers["Authorization"];
            if(jwt.Count == 0)
            {
                return _next(context);
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt.ToString().Replace("Bearer ", string.Empty));
            var issuer = token.Issuer;
            /*var loadBalancingOptions = proxyFeature.ClusterConfig.LoadBalancingOptions;

            var destination = _loadBalancer.PickDestination(destinations, in loadBalancingOptions);

            if (destination == null)
            {
                var cluster = context.GetRequiredCluster();
                Log.NoAvailableDestinations(_logger, cluster.ClusterId);
                context.Response.StatusCode = 503;
                return Task.CompletedTask;
            }
            */
            var destination = PickJWTIssuerDestination(destinations.Where(x => x.Config.Address.TrimEnd('/').Equals(issuer.TrimEnd('/'), StringComparison.Ordinal)));
            proxyFeature.AvailableDestinations = destination;
            
            return _next(context);
        }

        public DestinationInfo PickJWTIssuerDestination(
            IEnumerable<DestinationInfo> endpoints)
        {
            var endpointCount = endpoints.Count();
            if (endpointCount == 0)
            {
                return null;
            }

            if (endpointCount == 1)
            {
                return endpoints.First();
            }
            return endpoints.First();
        }


        private static class Log
        {
            private static readonly Action<ILogger, string, Exception> _noAvailableDestinations = LoggerMessage.Define<string>(
                LogLevel.Warning,
                EventIds.NoAvailableDestinations,
                "No available destinations after load balancing for cluster '{clusterId}'.");

            public static void NoAvailableDestinations(ILogger logger, string clusterId)
            {
                _noAvailableDestinations(logger, clusterId, null);
            }
        }
    }
}
