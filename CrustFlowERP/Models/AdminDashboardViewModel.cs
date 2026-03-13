using CrustFlowERP.Models.Sales;
using System.Collections.Generic;

namespace CrustFlowERP.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TodayOrdersCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public int LowStockItems { get; set; }
        public List<Sale> RecentSales { get; set; } = new List<Sale>();
    }
}
