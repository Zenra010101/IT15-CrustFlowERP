using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CrustFlowERP.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Role")]
        public int Role { get; set; } = 3; // Default to Cashier
        
        [Display(Name = "Assign Role")]
        public string SelectedRole { get; set; } = string.Empty;
        
        [Display(Name = "Account Status")]
        public bool IsLockedOut { get; set; }
        
        [Display(Name = "Lockout End")]
        public DateTimeOffset? LockoutEnd { get; set; }
        
        // For role dropdown
        public List<SelectListItem>? AllRoles { get; set; }
    }
}
