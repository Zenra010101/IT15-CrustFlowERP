using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrustFlowERP.Models.Production;

namespace CrustFlowERP.Models.Inventory
{
    public class InventoryWastage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Required]
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Notes { get; set; }

        [ForeignKey("InventoryId")]
        public virtual Production.Inventory? Inventory { get; set; }
    }
}
