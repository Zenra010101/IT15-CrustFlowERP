using System.ComponentModel.DataAnnotations;

namespace CrustFlowERP.Models
{
    public class UserActionModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}
