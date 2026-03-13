using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrustFlowERP.Models.Production
{
    public class Wastage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionOrderId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityLost { get; set; }

        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = "Burnt";

        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductionOrderId")]
        public virtual ProductionOrder? ProductionOrder { get; set; }
    }
}
