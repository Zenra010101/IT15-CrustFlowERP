using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrustFlowERP.Data;
using CrustFlowERP.Models.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace CrustFlowERP.Controllers
{
    [Authorize]
    public class ProductionOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.IPdfService _pdfService;
 
        public ProductionOrdersController(ApplicationDbContext context, Services.IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetInt32("UserRole");
            // Allow SuperAdmin (1), Admin (2), Production roles (3, 4, 5) or General Manager (11)
            return role.HasValue && (role.Value <= 5 || role.Value == 11);
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (!IsAuthorized())
            {
                context.Result = new RedirectToActionResult("Unauthorized", "Home", null);
            }
            base.OnActionExecuting(context);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadBatchSheet(int id)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateProductionOrderPdfAsync(id);
                if (pdfBytes == null) return NotFound();

                return File(pdfBytes, "application/pdf", $"ProductionSheet-Batch-{id:D4}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: ProductionOrders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.ProductionOrders
                .Include(p => p.Product)
                .OrderByDescending(p => p.ScheduledStart)
                .ToListAsync();
            return View(orders);
        }

        // GET: ProductionOrders/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => p.IsActive).Select(p => new { p.Id, DisplayName = p.Name + " [" + p.Category + "]" }), "Id", "DisplayName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,QuantityPlanned,ScheduledStart,Notes")] ProductionOrder order)
        {
            if (ModelState.IsValid)
            {
                order.Status = ProductionStatus.Planned;
                order.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Production order scheduled!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => p.IsActive), "Id", "Name", order.ProductId);
            return View(order);
        }

        // GET: ProductionOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.ProductionOrders
                .Include(o => o.Product)
                    .ThenInclude(p => p!.Recipes)
                        .ThenInclude(r => r.Ingredient)
                .Include(o => o.ProductionLogs)
                    .ThenInclude(l => l.Staff)
                .Include(o => o.QualityChecks)
                .Include(o => o.Wastages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: ProductionOrders/GetOrderDetailsContent/5
        public async Task<IActionResult> GetOrderDetailsContent(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.ProductionOrders
                .Include(o => o.Product)
                    .ThenInclude(p => p!.Recipes)
                        .ThenInclude(r => r.Ingredient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return PartialView("_DetailsPartial", order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, ProductionStatus status, decimal? actualQty = null)
        {
            try 
            {
                var order = await _context.ProductionOrders
                    .Include(o => o.Product)
                        .ThenInclude(p => p!.Recipes)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null) return Json(new { success = false, message = "Production order not found." });

                if (status == ProductionStatus.InProgress && order.ActualStart == null)
                {
                    order.ActualStart = DateTime.Now;
                    order.QuantityActual = order.QuantityPlanned;

                    // Standard way to get current logged-in user ID
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    await DeductIngredients(order, userId);
                }
                else if (status == ProductionStatus.Completed || status == ProductionStatus.QualityChecked)
                {
                    if (order.ActualEnd == null) order.ActualEnd = DateTime.Now;
                    if (actualQty.HasValue) order.QuantityActual = actualQty.Value;
                }
                else if (status == ProductionStatus.Approved)
                {
                    // If moving TO Approved from anything else, add the produced quantity to product stock
                    if (order.Status != ProductionStatus.Approved)
                    {
                        if (order.Product != null)
                        {
                            order.Product.StockQuantity += order.QuantityActual;
                            
                            // Log the stock addition
                            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                            _context.ProductionLogs.Add(new ProductionLog
                            {
                                ProductionOrderId = order.Id,
                                ActivityDescription = $"✅ STOCK UPDATED: Added {order.QuantityActual:N2} to {order.Product.Name} inventory.",
                                Timestamp = DateTime.Now,
                                StaffId = userId
                            });
                        }
                    }
                }

                order.Status = status;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log exception for server debugging
                Console.WriteLine($"ERROR in UpdateStatus: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task DeductIngredients(ProductionOrder order, string? userId)
        {
            if (order.Product?.Recipes == null) return;

            foreach (var recipe in order.Product.Recipes)
            {
                decimal quantityNeeded = order.QuantityPlanned * recipe.QuantityRequired;
                
                var inventoryItems = await _context.Inventories
                    .Where(i => i.IngredientId == recipe.IngredientId && !i.IsArchived && i.RemainingQuantity > 0)
                    .OrderBy(i => i.ExpirationDate)
                    .ToListAsync();

                foreach (var item in inventoryItems)
                {
                    if (quantityNeeded <= 0) break;

                    decimal toDeduct = Math.Min(item.RemainingQuantity, quantityNeeded);
                    item.RemainingQuantity -= toDeduct;
                    quantityNeeded -= toDeduct;

                    if (item.RemainingQuantity == 0)
                    {
                        item.IsArchived = true;
                    }
                }

                if (quantityNeeded > 0)
                {
                    var warningLog = new ProductionLog
                    {
                        ProductionOrderId = order.Id,
                        ActivityDescription = $"⚠️ INSUFFICIENT STOCK: Short by {quantityNeeded} for ingredient ID {recipe.IngredientId}",
                        Timestamp = DateTime.Now,
                        StaffId = userId 
                    };
                    _context.ProductionLogs.Add(warningLog);
                }
                else
                {
                    var ingredient = await _context.Ingredients.FindAsync(recipe.IngredientId);
                    var successLog = new ProductionLog
                    {
                        ProductionOrderId = order.Id,
                        ActivityDescription = $"Consumed {(order.QuantityPlanned * recipe.QuantityRequired):N2} {ingredient?.Unit} of {ingredient?.Name}",
                        Timestamp = DateTime.Now,
                        StaffId = userId
                    };
                    _context.ProductionLogs.Add(successLog);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> LogActivity(int id, string description)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var log = new ProductionLog
            {
                ProductionOrderId = id,
                ActivityDescription = description,
                Timestamp = DateTime.Now,
                StaffId = userId
            };

            _context.ProductionLogs.Add(log);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ReportWastage(int id, decimal quantity, string reason)
        {
            var order = await _context.ProductionOrders.FindAsync(id);
            if (order == null) return NotFound();

            var wastage = new Wastage
            {
                ProductionOrderId = id,
                QuantityLost = quantity,
                Reason = reason,
                ReportedAt = DateTime.Now
            };

            if (order.QuantityActual > 0)
            {
                order.QuantityActual = Math.Max(0, order.QuantityActual - quantity);
            }
            else if (order.Status == ProductionStatus.InProgress)
            {
                order.QuantityActual = Math.Max(0, order.QuantityPlanned - quantity);
            }

            _context.Wastages.Add(wastage);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> LogQualityCheck(int id, bool isPassed, string remarks)
        {
            var role = HttpContext.Session.GetInt32("UserRole");
            if (role != 5 && role != 11 && role != 1 && role != 2) 
            {
                return Json(new { success = false, message = "Only Quality Control Staff or Managers can perform quality checks." });
            }

            var userName = HttpContext.Session.GetString("UserEmail") ?? "System";
            var check = new QualityCheck
            {
                ProductionOrderId = id,
                IsPassed = isPassed,
                Remarks = remarks,
                CheckedAt = DateTime.Now,
                CheckedBy = userName
            };

            _context.QualityChecks.Add(check);

            var order = await _context.ProductionOrders.FindAsync(id);
            if (order != null && isPassed)
            {
                order.Status = ProductionStatus.QualityChecked;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
