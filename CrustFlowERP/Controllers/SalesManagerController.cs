using CrustFlowERP.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrustFlowERP.Data;
using Microsoft.EntityFrameworkCore;

namespace CrustFlowERP.Controllers
{
    [AuthorizeSalesManager]
    public class SalesManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.UserRole = "Sales Manager";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            return View();
        }

        public async Task<IActionResult> SalesReports()
        {
            ViewBag.UserRole = "Sales Manager";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var last30Days = today.AddDays(-30);

            // 1. Daily Revenue
            var dailyRevenue = await _context.Sales
                .Where(s => s.SaleDate >= today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            
            var yesterday = today.AddDays(-1);
            var yesterdayRevenue = await _context.Sales
                .Where(s => s.SaleDate >= yesterday && s.SaleDate < today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            
            ViewBag.DailyRevenue = dailyRevenue;
            ViewBag.RevenueGrowth = yesterdayRevenue > 0 ? ((dailyRevenue - yesterdayRevenue) / yesterdayRevenue) * 100 : 0;

            // 2. Weekly Sales Count
            ViewBag.WeeklySalesCount = await _context.Sales
                .CountAsync(s => s.SaleDate >= startOfWeek);
            
            // 3. Avg Order Value (Last 30 days)
            var totalSales30 = await _context.Sales
                .Where(s => s.SaleDate >= last30Days)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            
            var countSales30 = await _context.Sales
                .CountAsync(s => s.SaleDate >= last30Days);
            
            ViewBag.AvgOrderValue = countSales30 > 0 ? totalSales30 / countSales30 : 0;

            // 4. Revenue Trend (Last 7 days)
            var trendData = await _context.Sales
                .Where(s => s.SaleDate >= today.AddDays(-6))
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(s => s.TotalAmount) })
                .OrderBy(g => g.Date)
                .ToListAsync();
            
            ViewBag.TrendData = trendData;

            return View();
        }



        public IActionResult ProductionCoordination()
        {
            return View();
        }
    }
}
