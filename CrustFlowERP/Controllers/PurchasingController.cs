using CrustFlowERP.Attributes;
using CrustFlowERP.Data;
using CrustFlowERP.Models;
using CrustFlowERP.Models.Purchasing;
using CrustFlowERP.Models.Inventory;
using CrustFlowERP.Models.Production;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CrustFlowERP.Models.CRM;

namespace CrustFlowERP.Controllers
{
    [AuthorizePurchasingOfficer]
    public class PurchasingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.IPdfService _pdfService;
 
        public PurchasingController(ApplicationDbContext context, Services.IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPurchaseOrder(int id)
        {
            try
            {
                var pdfBytes = await _pdfService.GeneratePurchaseOrderPdfAsync(id);
                if (pdfBytes == null) return NotFound();

                var orderNumber = await _context.PurchaseOrders.Where(p => p.Id == id).Select(p => p.OrderNumber).FirstOrDefaultAsync();
                return File(pdfBytes, "application/pdf", $"PurchaseOrder-{orderNumber ?? id.ToString()}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName") ?? "Purchasing Officer";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var ingredients = await _context.Ingredients
                .Include(i => i.Inventories)
                .Where(i => i.IsActive)
                .ToListAsync();

            var lowStockIngredients = ingredients
                .Select(i => new {
                    i.Id,
                    i.Name,
                    CurrentStock = i.Inventories.Where(inv => !inv.IsArchived).Sum(inv => inv.RemainingQuantity),
                    i.MinimumStockLevel,
                    i.Unit
                })
                .Where(i => i.CurrentStock < i.MinimumStockLevel)
                .ToList();

            var poStats = new {
                TotalPending = await _context.PurchaseOrders.CountAsync(po => po.Status == PurchaseOrderStatus.Pending),
                TotalOrdered = await _context.PurchaseOrders.CountAsync(po => po.Status == PurchaseOrderStatus.Ordered),
                RecentOrders = await _context.PurchaseOrders.Include(po => po.Supplier).OrderByDescending(po => po.OrderDate).Take(5).ToListAsync(),
                LowStockAlerts = lowStockIngredients
            };

            ViewBag.Stats = poStats;
            
            return View();
        }

        // GET: Purchasing/PurchaseOrders
        public async Task<IActionResult> PurchaseOrders()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName") ?? "Purchasing Officer";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            var orders = await _context.PurchaseOrders.Include(po => po.Supplier).OrderByDescending(po => po.OrderDate).ToListAsync();
            return View(orders);
        }

        public IActionResult CreatePurchaseOrder()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName") ?? "Purchasing Officer";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.Suppliers = _context.Suppliers.Where(s => s.IsActive).ToList();
            ViewBag.Ingredients = _context.Ingredients.Where(i => i.IsActive).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePurchaseOrder(PurchaseOrder order, List<PurchaseOrderDetail> details)
        {
            if (details == null || !details.Any())
            {
                ModelState.AddModelError("", "You must add at least one ingredient to the purchase order.");
            }

            // OrderNumber is generated server-side, so we remove it from validation
            ModelState.Remove(nameof(order.OrderNumber));

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Generate Order Number: PO-YYYYMMDD-XXX
                    var todayStr = DateTime.Now.ToString("yyyyMMdd");
                    var countToday = await _context.PurchaseOrders
                        .CountAsync(po => po.OrderNumber.StartsWith("PO-" + todayStr));
                    order.OrderNumber = $"PO-{todayStr}-{(countToday + 1).ToString("D3")}";
                    
                    order.Status = PurchaseOrderStatus.Pending;
                    order.CreatedAt = DateTime.Now;
                    order.UpdatedAt = DateTime.Now;
                    order.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    
                    // Initial add to get the ID
                    _context.PurchaseOrders.Add(order);
                    await _context.SaveChangesAsync();

                    decimal grandTotal = 0;
                    foreach (var detail in details!)
                    {
                        detail.PurchaseOrderId = order.Id;
                        detail.TotalPrice = detail.Quantity * detail.UnitPrice;
                        detail.Status = PurchaseItemStatus.Pending;
                        _context.PurchaseOrderDetails.Add(detail);
                        grandTotal += detail.TotalPrice;
                    }

                    // Update total amount on the order
                    order.TotalAmount = grandTotal;
                    _context.PurchaseOrders.Update(order);
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Purchase Order {order.OrderNumber} created successfully!";
                    return RedirectToAction(nameof(PurchaseOrders));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "An error occurred while saving the purchase order: " + ex.Message);
                }
            }

            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            ViewBag.Ingredients = await _context.Ingredients.Where(i => i.IsActive).ToListAsync();
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName") ?? "Purchasing Officer";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            return View(order);
        }

        // GET: Purchasing/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderDetails)
                .ThenInclude(pod => pod.Ingredient)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null) return NotFound();

            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName") ?? "Purchasing Officer";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View(order);
        }

        // GET: Purchasing/Wastage
        public async Task<IActionResult> Wastage()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName") ?? "Purchasing Officer";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var wastages = await _context.InventoryWastages
                .Include(w => w.Inventory)
                .ThenInclude(i => i!.Ingredient)
                .Where(w => w.Inventory != null && w.Inventory.Category == InventoryCategory.RawMaterials)
                .OrderByDescending(w => w.ReportedAt)
                .ToListAsync();

            return View(wastages);
        }

        // GET: Purchasing/RecordWastage
        public async Task<IActionResult> RecordWastage()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName") ?? "Purchasing Officer";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            // Only show raw materials with remaining quantity
            ViewBag.Inventories = await _context.Inventories
                .Include(i => i.Ingredient)
                .Where(i => i.Category == InventoryCategory.RawMaterials && i.RemainingQuantity > 0)
                .OrderBy(i => i.IsArchived) // Show active first
                .ThenBy(i => i.Ingredient!.Name)
                .ToListAsync();

            return View();
        }

        // POST: Purchasing/RecordWastage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordWastage(InventoryWastage wastage)
        {
            if (ModelState.IsValid)
            {
                var inventory = await _context.Inventories.FindAsync(wastage.InventoryId);
                if (inventory != null)
                {
                    if (wastage.Quantity > inventory.RemainingQuantity)
                    {
                        ModelState.AddModelError("Quantity", "Wastage quantity cannot exceed remaining stock.");
                    }
                    else
                    {
                        inventory.RemainingQuantity -= wastage.Quantity;
                        _context.Update(inventory);
                        
                        wastage.ReportedAt = DateTime.UtcNow;
                        _context.InventoryWastages.Add(wastage);
                        
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Wastage recorded successfully. Inventory updated.";
                        return RedirectToAction(nameof(Wastage));
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Selected inventory batch not found.");
                }
            }

            ViewBag.Inventories = await _context.Inventories
                .Include(i => i.Ingredient)
                .Where(i => !i.IsArchived && i.RemainingQuantity > 0)
                .OrderBy(i => i.Ingredient!.Name)
                .ToListAsync();
            
            return View(wastage);
        }
    }
}
