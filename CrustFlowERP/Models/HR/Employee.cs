using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrustFlowERP.Data;

namespace CrustFlowERP.Models.HR
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

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

        public string? Position { get; set; }
        
        public string? Department { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.Now;

        public string? Status { get; set; } = "Active"; // Active, Resigned, OnLeave

        public string? ContactNumber { get; set; }
        
        public string? Address { get; set; }
        
        public string? EmergencyContact { get; set; }
        
        public string? EmergencyPhone { get; set; }

        public virtual ICollection<Attendance>? Attendances { get; set; }
        public virtual ICollection<Payroll>? Payrolls { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
