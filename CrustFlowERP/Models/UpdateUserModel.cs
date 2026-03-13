using System.ComponentModel.DataAnnotations;

namespace CrustFlowERP.Models
{
    public class UpdateUserModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
