using CrustFlowERP.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrustFlowERP.Data;
using CrustFlowERP.Models.Production;

namespace CrustFlowERP.Controllers
{
    [AuthorizeQualityControlStaff]
    public class QualityControlController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QualityControlController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserRole = "Quality Control Staff";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            // Batches waiting for inspection
            var pendingOrders = await _context.ProductionOrders
                .Include(p => p.Product)
                .Where(p => p.Status == ProductionStatus.Completed)
                .OrderByDescending(p => p.ActualEnd)
                .ToListAsync();

            // Recent inspections
            var recentChecks = await _context.QualityChecks
                .Include(qc => qc.ProductionOrder)
                    .ThenInclude(p => p!.Product)
                .OrderByDescending(qc => qc.CheckedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.PendingOrders = pendingOrders;
            ViewBag.RecentChecks = recentChecks;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogInpsection(int orderId, bool isPassed, string remarks, decimal? wastageQty = null, string? wastageReason = null)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "QC System";
            var order = await _context.ProductionOrders.FindAsync(orderId);
            
            if (order == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Create QC Record
                var check = new QualityCheck
                {
                    ProductionOrderId = orderId,
                    IsPassed = isPassed,
                    Remarks = remarks,
                    CheckedAt = DateTime.Now,
                    CheckedBy = userEmail
                };
                _context.QualityChecks.Add(check);

                // 2. Log Wastage if reported
                if (wastageQty.HasValue && wastageQty.Value > 0)
                {
                    var wastage = new Wastage
                    {
                        ProductionOrderId = orderId,
                        QuantityLost = wastageQty.Value,
                        Reason = wastageReason ?? "Defects found during QC",
                        ReportedAt = DateTime.Now
                    };
                    _context.Wastages.Add(wastage);

                    // Reduce actual quantity
                    order.QuantityActual = Math.Max(0, order.QuantityActual - wastageQty.Value);
                }

                // 3. Update Order Status
                if (isPassed)
                {
                    order.Status = ProductionStatus.QualityChecked;
                }
                // If it fails, maybe it stays in 'Completed' or we add a 'Failed' status? 
                // For now, let's keep it as is, maybe update notes.
                else
                {
                    order.Notes = (order.Notes ?? "") + $"\nFAILED QC on {DateTime.Now}: {remarks}";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = isPassed ? "Batch marked as Passed Quality Check!" : "Inspection logged. Batch marked as Failed.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Error logging inspection: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
