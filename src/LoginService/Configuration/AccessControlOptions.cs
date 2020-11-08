using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LoginService.Configuration
{
    public class AccessControlOptions : IValidatableObject
    {
        [Range(0, int.MaxValue)]
        public int LoginAttemptsCount { get; set; }

        public string LockoutTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(LockoutTime))
            {
                yield return new ValidationResult(
                    $"The {nameof(LockoutTime)} configuration is required",
                    new[] { nameof(LockoutTime) });
            }
            if (!TimeSpan.TryParse(LockoutTime, out _))
            {
                yield return new ValidationResult(
                    $"The {nameof(LockoutTime)} configuration must represent a TimeSpan",
                    new[] { nameof(LockoutTime) });
            }
        }
    }
}
