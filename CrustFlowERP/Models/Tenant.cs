using System.ComponentModel.DataAnnotations;

namespace CrustFlowERP.Models
{
    public class Tenant
    {
        [Key]
        public int TenantId { get; set; }

        [Required]
        [StringLength(200)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        public CompanyTier DatabaseName { get; set; } // Tier identifier as requested

        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        public bool IsArchived { get; set; } = false;
        public bool IsPaid { get; set; } = false;
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Monthly;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? NextDueDate { get; set; }
    }
}
