using System.Collections.Generic;
using System.Linq;

using global::Microsoft.AspNetCore.Authorization;
using global::Swashbuckle.AspNetCore.Swagger;
using global::Swashbuckle.AspNetCore.SwaggerGen;

using Microsoft.OpenApi.Models;

namespace ResourceService
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // https://www.thecodebuzz.com/oauth2-authorize-ioperationfilter-swaggeropenapi-asp-net-core/
            var isAuthorized = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                           context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (!isAuthorized)
            {
                return;
            }

            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            var oauth2SecurityScheme = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" },
            };

            operation.Security.Add(new OpenApiSecurityRequirement()
            {
                [oauth2SecurityScheme] = new[] { "thecodebuzz" } //'thecodebuzz' is scope here

            });
        }
    }
}
