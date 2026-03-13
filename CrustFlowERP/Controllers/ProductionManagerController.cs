using CrustFlowERP.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using CrustFlowERP.Data;
using CrustFlowERP.Models.Production;
using Microsoft.EntityFrameworkCore;

namespace CrustFlowERP.Controllers
{
    [AuthorizeProductionManager]
    public class ProductionManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductionManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserRole = "Production Manager";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var today = DateTime.Today;
            
            ViewBag.ActiveOrders = await _context.ProductionOrders
                .CountAsync(o => o.Status == ProductionStatus.Planned || o.Status == ProductionStatus.InProgress);
            
            ViewBag.BatchesToday = await _context.ProductionOrders
                .CountAsync(o => o.ScheduledStart.Date == today || (o.ActualStart.HasValue && o.ActualStart.Value.Date == today));

            ViewBag.WastageToday = await _context.Wastages
                .Where(w => w.ReportedAt.Date == today)
                .SumAsync(w => (decimal?)w.QuantityLost) ?? 0;

            ViewBag.RecentLogs = await _context.ProductionLogs
                .Include(l => l.ProductionOrder)
                    .ThenInclude(o => o!.Product)
                .OrderByDescending(l => l.Timestamp)
                .Take(10)
                .ToListAsync();

            return View();
        }
        
        public IActionResult Inventory()
        {
            return View();
        }
        
        public IActionResult ProductionBatch()
        {
            return View();
        }
        
        public IActionResult StaffEfficiency()
        {
            return View();
        }
        
        public async Task<IActionResult> Reports()
        {
            ViewBag.UserRole = "Production Manager";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            // 1. Efficiency: Actual vs Planned Quantity
            var prodStats = await _context.ProductionOrders
                .Where(o => o.ActualEnd >= thirtyDaysAgo)
                .Select(o => new { o.QuantityPlanned, o.QuantityActual })
                .ToListAsync();

            decimal totalPlanned = prodStats.Sum(s => s.QuantityPlanned);
            decimal totalActual = prodStats.Sum(s => s.QuantityActual);
            ViewBag.EfficiencyRate = totalPlanned > 0 ? (totalActual / totalPlanned) * 100 : 0;

            // 2. Quality: Pass Rate
            var qualityChecks = await _context.QualityChecks
                .Where(q => q.CheckedAt >= thirtyDaysAgo)
                .ToListAsync();
            
            int totalChecks = qualityChecks.Count;
            int passedChecks = qualityChecks.Count(q => q.IsPassed);
            ViewBag.QualityPassRate = totalChecks > 0 ? (passedChecks / (decimal)totalChecks) * 100 : 0;

            // 3. Wastage: % of Total Produced
            var totalWasted = await _context.Wastages
                .Where(w => w.ReportedAt >= thirtyDaysAgo)
                .SumAsync(w => (decimal?)w.QuantityLost) ?? 0;
            
            ViewBag.WastagePercentage = totalActual > 0 ? (totalWasted / (totalActual + totalWasted)) * 100 : 0;

            return View();
        }
    }
}
