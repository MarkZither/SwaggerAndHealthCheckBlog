using System;
using LoginService.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Shared.Extensions;

namespace LoginService.Validation
{
    /// <summary>
    /// Validates the LoginService's options, used at startup.
    /// </summary>
    public class ValidateStartupOptions : IValidateStartupOptions
    {
        private readonly IOptions<LoginOptions> _root;
        private readonly IOptions<AuthPasswordValidationOptions> _authPassword;
        private readonly IOptions<SigningCertificateOptions> _signingCert;
        private readonly IOptions<AccessControlOptions> _accessControl;
        private readonly ILogger<ValidateStartupOptions> _logger;

        public ValidateStartupOptions(
            IOptions<LoginOptions> root,
            IOptions<AuthPasswordValidationOptions> authPassword,
            IOptions<SigningCertificateOptions> signingCert,
            IOptions<AccessControlOptions> accessControl,
            ILogger<ValidateStartupOptions> logger)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _authPassword = authPassword ?? throw new ArgumentNullException(nameof(authPassword));
            _signingCert = signingCert ?? throw new ArgumentNullException(nameof(signingCert));
            _accessControl = accessControl ?? throw new ArgumentNullException(nameof(accessControl));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool Validate()
        {
            try
            {
                // Access each option to force validations to run.
                // Invalid options will trigger an "OptionsValidationException" exception.
                _ = _root.Value;
                _ = _authPassword.Value;
                _ = _signingCert.Value;
                _ = _accessControl.Value;

                return true;
            }
            catch (OptionsValidationException e)
            {
                foreach (var failure in e.Failures)
                {
                    _logger.LogError("{OptionsFailure}", failure);
                }

                _logger.LogError(e, "BaGet configuration is invalid.");
                return false;
            }
        }
    }
}
