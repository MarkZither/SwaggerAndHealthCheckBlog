using System.ComponentModel.DataAnnotations;

namespace ResourceService.Configuration
{
    public class ResourceOptions
    {
        public const string Resource = "Resource";
        public LoggingOptions Logging { get; set; }
        public string Urls { get; set; }
        public string ManagementPort { get; set; }
        [Required]
        [Url]
        public string IdentityServerUrl { get; set; }
        public HealthchecksUI HealthChecksUI { get; set; }
        public Connectionstrings ConnectionStrings { get; set; }
        public TokenCreationOptions TokenCreationOptions { get; set; }
        public CorssettingsOptions CorsSettings { get; set; }
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
}
