using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrustFlowERP.Data;

namespace CrustFlowERP.Models.Sales
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string CashierId { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PaymentMethod { get; set; } = "Cash";


        [ForeignKey("CashierId")]
        public virtual ApplicationUser? Cashier { get; set; }

        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual CRM.Customer? Customer { get; set; }

        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}
