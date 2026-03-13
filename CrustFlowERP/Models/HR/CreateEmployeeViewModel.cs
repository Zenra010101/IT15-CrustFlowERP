using System.ComponentModel.DataAnnotations;

namespace CrustFlowERP.Models.HR
{
    public class CreateEmployeeViewModel
    {
        [Required]
        [StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;

        public string? Department { get; set; }

        [Required]
        public decimal BaseSalary { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.Now;

        public string? ContactNumber { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
    }
}
