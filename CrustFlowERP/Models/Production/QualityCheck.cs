using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrustFlowERP.Models.Production
{
    public class QualityCheck
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionOrderId { get; set; }

        [Required]
        public bool IsPassed { get; set; }

        [Required]
        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;

        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string CheckedBy { get; set; } = string.Empty;

        [ForeignKey("ProductionOrderId")]
        public virtual ProductionOrder? ProductionOrder { get; set; }
    }
}
