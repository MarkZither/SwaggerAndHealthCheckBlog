using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace LoginService.Configuration
{
    public class SigningCertificateOptions : IValidatableObject
    {
        // make this an enum?
        public string CertificateStoreLocation { get; set; }
        // make this an enum?
        public string CertificateStoreName { get; set; }
        [Required]
        public string CertificateThumbprint { get; set; }
        [Required]
        public string CertificateSecurityAlgorithm { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Uri.TryCreate(CertificateSecurityAlgorithm, UriKind.Absolute, out _))
            {
                yield return new ValidationResult(
                $"The {nameof(CertificateSecurityAlgorithm)} configuration must be a url ",
                new[] { nameof(CertificateSecurityAlgorithm) });
            }
            if (Enum.TryParse<StoreLocation>(CertificateStoreLocation, out _))
            {
                yield return new ValidationResult(
                $"The {nameof(CertificateStoreLocation)} configuration must represent a System.Security.Cryptography.X509Certificates.StoreLocation (CurrentUser or LocalMachine)",
                new[] { nameof(CertificateStoreLocation) });
            }
            if (Enum.TryParse<StoreName>(CertificateStoreName, out _))
            {
                yield return new ValidationResult(
                $"The {nameof(CertificateStoreName)} configuration must represent a System.Security.Cryptography.X509Certificates.StoreName (Usually My or Root)",
                new[] { nameof(CertificateStoreName) });
            }
        }
    }
}
