using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Shared.Validation;

namespace Services.Shared.Extensions
{
    public static class IHostExtensions
    {
        public static bool ValidateStartupOptions(this IHost host)
        {
            return host
                .Services
                .GetRequiredService<IValidateStartupOptions>()
                .Validate();
        }
    }
}
