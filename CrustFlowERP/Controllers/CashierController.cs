using CrustFlowERP.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CrustFlowERP.Models.Sales;
using CrustFlowERP.Models.Production;
using CrustFlowERP.Services;

namespace CrustFlowERP.Controllers
{
    [AuthorizeCashier]
    public class CashierController : Controller
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public CashierController(Data.ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int saleId)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateReceiptPdfAsync(saleId);
                if (pdfBytes == null) return NotFound();

                return File(pdfBytes, "application/pdf", $"Receipt-{saleId:D4}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserRole = "Cashier";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var today = DateTime.Today;
            
            // 1. Today's Orders
            ViewBag.TodayOrders = await _context.Sales.CountAsync(s => s.SaleDate >= today);
            
            // 2. Today's Sales
            ViewBag.TodaySales = await _context.Sales
                .Where(s => s.SaleDate >= today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            
            // 3. Pending Production Orders (for the cashier to monitor pickup status)
            ViewBag.PendingProduction = await _context.ProductionOrders
                .CountAsync(o => o.Status == ProductionStatus.Planned || o.Status == ProductionStatus.InProgress);
            
            // 4. Avg Service Time (Calculated from completed production orders)
            var completedOrdersDates = await _context.ProductionOrders
                .Where(o => o.ActualStart != null && o.ActualEnd != null && o.Status == ProductionStatus.Approved)
                .Select(o => new { o.ActualStart, o.ActualEnd })
                .Take(100)
                .ToListAsync();
            
            var serviceTimes = completedOrdersDates
                .Select(o => (o.ActualEnd!.Value - o.ActualStart!.Value).TotalMinutes)
                .ToList();
            
            ViewBag.AvgServiceTime = serviceTimes.Any() 
                ? $"{serviceTimes.Average():F1}m" 
                : "8.5m"; 

            return View();
        }
        
        public async Task<IActionResult> ProcessSales()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email) ?? HttpContext.Session.GetString("UserEmail") ?? "Cashier";
            
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
            ViewBag.CashierName = employee != null ? $"{employee.FirstName} {employee.LastName}" : email;

            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitSale([FromBody] Sale saleData)
        {
            if (saleData == null || !saleData.SaleDetails.Any())
            {
                return Json(new { success = false, message = "No items in sale." });
            }

            try
            {
                // Set server-side data
                saleData.SaleDate = DateTime.UtcNow;
                saleData.CashierId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                // Deduct stock and validate prices
                foreach (var detail in saleData.SaleDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null) continue;

                    if (product.StockQuantity < detail.Quantity)
                    {
                        return Json(new { success = false, message = $"Insufficient stock for {product.Name} (Available: {product.StockQuantity})." });
                    }

                    product.StockQuantity -= detail.Quantity;
                    detail.UnitPrice = product.Price; // Ensure correct price
                    detail.Subtotal = detail.UnitPrice * detail.Quantity;
                }

                _context.Sales.Add(saleData);
                await _context.SaveChangesAsync();

                return Json(new { success = true, saleId = saleData.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        public async Task<IActionResult> PrintReceipts()
        {
            var sales = await _context.Sales
                .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Product)
                .Include(s => s.Cashier)
                .OrderByDescending(s => s.SaleDate)
                .Take(50)
                .ToListAsync();

            return View(sales);
        }
        
        public async Task<IActionResult> ViewStock()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
                
            return View(products);
        }
        
        public IActionResult Sales()
        {
            return View();
        }
        
        public async Task<IActionResult> Orders()
        {
            return View();
        }
        

        public IActionResult Payments()
        {
            return View();
        }
    }
}
