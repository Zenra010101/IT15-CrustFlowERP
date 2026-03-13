using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrustFlowERP.Models.CRM;
using CrustFlowERP.Models;

namespace CrustFlowERP.Models.Production
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = "kg"; // e.g., kg, liters, grams, pieces

        public bool IsActive { get; set; } = true;

        public decimal MinimumStockLevel { get; set; }

        public SupplyCategory Category { get; set; }
        public InventoryCategory InventoryCategory { get; set; } = InventoryCategory.RawMaterials;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}
