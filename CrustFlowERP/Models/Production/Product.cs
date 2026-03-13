using System.ComponentModel.DataAnnotations;

namespace CrustFlowERP.Models.Production
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "Bread";

        [Required]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public decimal StockQuantity { get; set; } = 0;

        [Required]
        public decimal MinStockLevel { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        // Navigation properties
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public virtual ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
    }
}
