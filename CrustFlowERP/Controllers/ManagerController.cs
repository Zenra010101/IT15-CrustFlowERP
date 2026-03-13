using CrustFlowERP.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrustFlowERP.Data;
using Microsoft.EntityFrameworkCore;
using CrustFlowERP.Models;
using CrustFlowERP.Models.Production;
using CrustFlowERP.Models.HR;
using CrustFlowERP.Models.Purchasing;

namespace CrustFlowERP.Controllers
{
    [AuthorizeGeneralManager]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var last30Days = today.AddDays(-30);

            // 1. Core Operational Metrics
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => p.IsActive);
            
            // Refined Low Stock Logic
            var ingredients = await _context.Ingredients
                .Where(i => i.IsActive)
                .Select(i => new IngredientStockDto { 
                    Id = i.Id,
                    Name = i.Name,
                    MinimumStockLevel = i.MinimumStockLevel,
                    Unit = i.Unit,
                    Stock = _context.Inventories.Where(inv => inv.IngredientId == i.Id).Sum(inv => (double?)inv.RemainingQuantity) ?? 0 
                })
                .ToListAsync();
            
            var lowStockCount = ingredients.Count(x => x.MinimumStockLevel > (decimal)x.Stock);
            ViewBag.LowStockCount = lowStockCount;
            ViewBag.CriticalIngredients = ingredients.Where(x => x.MinimumStockLevel > (decimal)x.Stock).Take(5).ToList();

            ViewBag.ActiveProduction = await _context.ProductionOrders
                .CountAsync(o => o.Status == ProductionStatus.InProgress || o.Status == ProductionStatus.Approved);
            
            ViewBag.TotalEmployees = await _context.Employees.CountAsync(e => e.Status == "Active");
            
            // 2. Financial Velocity
            var salesToday = await _context.Sales
                .Where(s => s.SaleDate >= today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            ViewBag.SalesToday = salesToday;

            var salesTarget = 15000m; // Daily baseline target
            ViewBag.SalesTargetProgress = salesToday > 0 ? Math.Min(100m, (salesToday / salesTarget) * 100m) : 0m;

            // 3. Efficiency Intelligence
            // - Production Yield (Actual vs Planned)
            var productionData = await _context.ProductionOrders
                .Where(o => o.ActualEnd >= last30Days && o.Status == ProductionStatus.Completed)
                .Select(o => new { o.QuantityPlanned, o.QuantityActual })
                .ToListAsync();
            
            decimal totalPlanned = productionData.Sum(o => o.QuantityPlanned);
            decimal totalActual = productionData.Sum(o => o.QuantityActual);
            ViewBag.ProductionEfficiency = totalPlanned > 0 ? (totalActual / totalPlanned) * 100m : 100m;

            // - Quality Integrity (QC Pass Rate)
            var qualityChecks = await _context.QualityChecks
                .Where(qc => qc.CheckedAt >= last30Days)
                .ToListAsync();
            int totalChecks = qualityChecks.Count;
            int passedChecks = qualityChecks.Count(qc => qc.IsPassed);
            ViewBag.QualityPassRate = totalChecks > 0 ? (passedChecks / (decimal)totalChecks) * 100m : 100m;

            // - Inventory Health (Sustainability Index)
            var totalIngredients = await _context.Ingredients.CountAsync(i => i.IsActive);
            ViewBag.InventoryHealth = totalIngredients > 0 ? ((totalIngredients - lowStockCount) / (decimal)totalIngredients) * 100m : 100m;

            // 4. Live Stream
            ViewBag.RecentProduction = await _context.ProductionOrders
                .Include(o => o.Product)
                .OrderByDescending(o => o.ScheduledStart)
                .Take(6)
                .ToListAsync();

            ViewBag.UserRole = "Executive Manager";
            ViewBag.UserEmail = HttpContext.Session?.GetString("UserEmail") ?? "manager@crustflow.io";
            
            return View();
        }

        public async Task<IActionResult> Reports()
        {
            var today = DateTime.Now;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            
            // 1. Revenue Dynamics
            var revenue = await _context.Sales
                .Where(s => s.SaleDate >= startOfMonth && s.SaleDate <= endOfMonth)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            ViewBag.RevenueThisMonth = revenue;

            // 2. Cost Analysis (CapEx & OpEx)
            var ingestionCosts = await _context.PurchaseOrders
                .Where(po => po.OrderDate >= startOfMonth && po.OrderDate <= endOfMonth && po.Status == PurchaseOrderStatus.Received)
                .SumAsync(po => (decimal?)po.TotalAmount) ?? 0;
            
            var payrollOpEx = await _context.Payrolls
                .Where(p => p.PaymentDate >= startOfMonth && p.PaymentDate <= endOfMonth && p.Status == "Paid")
                .SumAsync(p => (decimal?)(p.BasePay + p.OvertimePay - p.Deductions)) ?? 0;

            ViewBag.PurchaseCosts = ingestionCosts;
            ViewBag.PayrollCosts = payrollOpEx;
            
            // Dynamic Utilities Estimation (e.g., 5% of revenue for demonstration)
            var estimatedUtilities = revenue * 0.05m;
            ViewBag.UtilityCosts = estimatedUtilities;

            var totalOpEx = ingestionCosts + payrollOpEx + estimatedUtilities;
            ViewBag.TotalExpenses = totalOpEx;
            ViewBag.NetProfit = revenue - totalOpEx;

            // 3. Strategic KPIs
            // - Market Penetration (Sales Growth Target)
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);
            var lastMonthRevenue = await _context.Sales
                .Where(s => s.SaleDate >= startOfLastMonth && s.SaleDate <= endOfLastMonth)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 5000m; // default baseline

            var strategicTarget = Math.Max(lastMonthRevenue * 1.15m, 12000m);
            ViewBag.SalesTarget = strategicTarget;
            ViewBag.SalesTargetProgress = (revenue / strategicTarget) * 100;
            
            // - Operational Margin
            ViewBag.OperationalMargin = revenue > 0 ? ((revenue - totalOpEx) / revenue) * 100m : 0m;

            // - Wastage Delta
            var totalCompletedQty = await _context.ProductionOrders
                .Where(o => o.ActualEnd >= startOfMonth)
                .SumAsync(o => (decimal?)o.QuantityActual) ?? 1;
            
            var totalWastageQty = await _context.Wastages
                .Where(w => w.ReportedAt >= startOfMonth)
                .SumAsync(w => (decimal?)w.QuantityLost) ?? 0;
            
            ViewBag.WastePercentage = (totalWastageQty / (totalCompletedQty + totalWastageQty)) * 100m;

            // - Human Capital Utilization (Presence Rate today)
            var activeEmployees = await _context.Employees.CountAsync(e => e.Status == "Active");
            var attendanceToday = await _context.Attendances.CountAsync(a => a.Date == today.Date);
            ViewBag.PresenceRate = activeEmployees > 0 ? (attendanceToday / (decimal)activeEmployees) * 100m : 0m;

            ViewBag.UserEmail = HttpContext.Session?.GetString("UserEmail") ?? "manager@crustflow.io";

            return View();
        }
    }
}
