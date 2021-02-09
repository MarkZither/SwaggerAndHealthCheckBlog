using System;
using System.ComponentModel.DataAnnotations;

namespace LoginService.Configuration
{
    public class AuthPasswordValidationOptions
    {
        [Required]
        [Range(2, int.MaxValue)]
        public int MaximumPasswordLength { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int MinimumPasswordLength { get; set; }
    }
}
