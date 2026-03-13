using System.ComponentModel.DataAnnotations;

namespace CrustFlowERP.Models
{
    public class ChangePasswordModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
