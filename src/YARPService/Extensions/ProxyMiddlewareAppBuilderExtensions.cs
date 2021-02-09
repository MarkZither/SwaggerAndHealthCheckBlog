using Microsoft.AspNetCore.Builder;

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
