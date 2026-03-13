using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrustFlowERP.Models;
using CrustFlowERP.Models.CRM;

namespace CrustFlowERP.Models.Production
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IngredientId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal BatchQuantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal RemainingQuantity { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? BatchNumber { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        [ForeignKey("IngredientId")]
        public virtual Ingredient? Ingredient { get; set; }

        public InventoryCategory Category { get; set; }
        public bool IsArchived { get; set; } = false;
    }
}
