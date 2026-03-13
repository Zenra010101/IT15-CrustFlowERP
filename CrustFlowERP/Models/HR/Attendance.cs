using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrustFlowERP.Models.HR
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        // Morning Session
        public DateTime? CheckInMorning { get; set; }
        public DateTime? CheckOutMorning { get; set; }

        // Afternoon Session
        public DateTime? CheckInAfternoon { get; set; }
        public DateTime? CheckOutAfternoon { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } = "Present"; // Present, Late, Absent, HalfDay

        public string? Notes { get; set; }

        [NotMapped]
        public double DurationHours
        {
            get
            {
                double hours = 0;
                if (CheckInMorning.HasValue && CheckOutMorning.HasValue)
                {
                    hours += (CheckOutMorning.Value - CheckInMorning.Value).TotalHours;
                }
                if (CheckInAfternoon.HasValue && CheckOutAfternoon.HasValue)
                {
                    hours += (CheckOutAfternoon.Value - CheckInAfternoon.Value).TotalHours;
                }
                return hours;
            }
        }
    }
}
