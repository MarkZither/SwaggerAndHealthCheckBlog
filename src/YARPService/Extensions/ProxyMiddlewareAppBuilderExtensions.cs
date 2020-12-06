using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YARPService.Middlewares
{
    public static class ProxyMiddlewareAppBuilderExtensions
    {
        /// <summary>
        /// Load balances across the available endpoints.
        /// </summary>
        public static IApplicationBuilder UseJWTIssuerRouter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JWTIssuerRouterMiddleware>();
        }
    }
}
