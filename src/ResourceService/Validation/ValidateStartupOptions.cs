using ResourceService.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Shared.Validation;
using System;

namespace ResourceService.Validation
{
    /// <summary>
    /// Validates the LoginService's options, used at startup.
    /// </summary>
    public class ValidateStartupOptions : IValidateStartupOptions
    {
        private readonly IOptions<ResourceOptions> _root;

        private readonly ILogger<ValidateStartupOptions> _logger;

        public ValidateStartupOptions(
            IOptions<ResourceOptions> root,
            ILogger<ValidateStartupOptions> logger)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool Validate()
        {
            try
            {
                // Access each option to force validations to run.
                // Invalid options will trigger an "OptionsValidationException" exception.
                _ = _root.Value;

                return true;
            }
            catch (OptionsValidationException e)
            {
                foreach (var failure in e.Failures)
                {
                    _logger.LogError("{OptionsFailure}", failure);
                }

                _logger.LogError(e, "Resource configuration is invalid.");
                return false;
            }
        }
    }
}
