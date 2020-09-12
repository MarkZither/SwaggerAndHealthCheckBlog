using Microsoft.AspNetCore.Builder;
using System;

namespace Services.Shared.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerUrlPortAuthMiddleware(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<SwaggerUrlPortAuthMiddleware>();
        }
    }
}
