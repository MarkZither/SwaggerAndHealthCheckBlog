using System.ComponentModel.DataAnnotations;

namespace LoginService.Configuration
{
    public class PasswordValidationOptions
    {
        [Required]
        public string PasswordRegexp { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MaxLength { get; set; }
    }
}
