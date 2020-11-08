using System.ComponentModel.DataAnnotations;

namespace LoginService.Configuration
{
    public class LoginOptions
    {
        public LoggingOptions Logging { get; set; }
        public string Urls { get; set; }
        public string ManagementPort { get; set; }
        public HealthchecksUI HealthChecksUI { get; set; }
        public Connectionstrings ConnectionStrings { get; set; }
        public SigningCertificateOptions SigningCertificate { get; set; }
        public AuthPasswordValidationOptions AuthPasswordValidationOptions { get; set; }
        public TokenCreationOptions TokenCreationOptions { get; set; }
        public CorssettingsOptions CorsSettings { get; set; }
        public UserManagementConnectionSettingsOptions UserManagementConnectionSettings { get; set; }
        public PasswordValidationOptions PasswordValidationOptions { get; set; }
        public AccessControlOptions AccessControlSettings { get; set; }
    }

    public class LoggingOptions
    {
        public bool IncludeScopes { get; set; }
        public Loglevel LogLevel { get; set; }
    }

    public class Loglevel
    {
        public string Default { get; set; }
        public string System { get; set; }
        public string Microsoft { get; set; }
    }

    public class HealthchecksUI
    {
        public Healthcheck[] HealthChecks { get; set; }
    }

    public class Healthcheck
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }

    public class Connectionstrings
    {
        public string LoginServiceDb { get; set; }
    }

    public class TokenCreationOptions
    {
        public string TokenLifetime { get; set; }
    }

    public class CorssettingsOptions
    {
        public string Origin { get; set; }
    }

    public class UserManagementConnectionSettingsOptions
    {
        public string ServiceAddress { get; set; }
    }

}
