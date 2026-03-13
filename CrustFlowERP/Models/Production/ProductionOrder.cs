using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrustFlowERP.Data;

namespace CrustFlowERP.Models.Production
{
    public enum ProductionStatus
    {
        Planned,
        InProgress,
        Completed,
        Cancelled,
        QualityChecked,
        Approved
    }

    public class ProductionOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityPlanned { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityActual { get; set; }

        [Required]
        public DateTime ScheduledStart { get; set; }

        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }

        [Required]
        public ProductionStatus Status { get; set; } = ProductionStatus.Planned;

        [StringLength(255)]
        public string? Notes { get; set; }

        public string? CreatedById { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser? CreatedBy { get; set; }

        public virtual ICollection<ProductionLog> ProductionLogs { get; set; } = new List<ProductionLog>();
        public virtual ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
        public virtual ICollection<Wastage> Wastages { get; set; } = new List<Wastage>();
    }
}
