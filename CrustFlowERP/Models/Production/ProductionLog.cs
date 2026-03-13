using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrustFlowERP.Data;

namespace CrustFlowERP.Models.Production
{
    public class ProductionLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionOrderId { get; set; }

        public string? StaffId { get; set; }

        [Required]
        [StringLength(255)]
        public string ActivityDescription { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductionOrderId")]
        public virtual ProductionOrder? ProductionOrder { get; set; }

        [ForeignKey("StaffId")]
        public virtual ApplicationUser? Staff { get; set; }
    }
}
