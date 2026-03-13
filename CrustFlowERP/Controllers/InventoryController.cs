using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrustFlowERP.Data;
using CrustFlowERP.Models.Production;
using CrustFlowERP.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

using CrustFlowERP.Attributes;

namespace CrustFlowERP.Controllers
{
    [AuthorizeWarehouseStaff]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Inventory
        public async Task<IActionResult> Index()
        {
            // Data Fix: Set all Inventory records with category 0 (unset) to RawMaterials (1)
            var legacyInventory = await _context.Inventories
                .Where(i => (int)i.Category == 0)
                .ToListAsync();
            if (legacyInventory.Any())
            {
                foreach (var i in legacyInventory) i.Category = CrustFlowERP.Models.InventoryCategory.RawMaterials;
                await _context.SaveChangesAsync();
            }

            var inventory = await _context.Inventories
                .Include(i => i.Ingredient)
                .Where(i => !i.IsArchived)
                .OrderBy(i => i.ExpirationDate)
                .ToListAsync();
            return View(inventory);
        }

        // GET: Inventory/Create
        public async Task<IActionResult> Create()
        {
            var suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            var supplierList = suppliers.Select(s => new { 
                Id = s.Id, 
                DisplayName = s.Name 
            });

            var ingredients = await _context.Ingredients
                .Where(i => i.IsActive)
                .OrderBy(i => i.InventoryCategory)
                .ThenBy(i => i.Name)
                .Select(i => new {
                    Id = i.Id,
                    DisplayName = i.Name
                })
                .ToListAsync();

            ViewData["IngredientId"] = new SelectList(ingredients, "Id", "DisplayName");
            ViewData["SupplierId"] = new SelectList(supplierList, "Id", "DisplayName");
            return View();
        }

        // GET: Inventory/GetIngredientsBySupplier/5
        [HttpGet]
        public async Task<IActionResult> GetIngredientsBySupplier(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null) return Json(new List<object>());

            // Fetch ingredients that match the supplier's MainProductType
            // but show their InventoryCategory (Raw, Equipment, etc.)
            var ingredients = await _context.Ingredients
                .Where(i => i.Category == supplier.MainProductType && i.IsActive)
                .Select(i => new { 
                    id = i.Id, 
                    name = i.Name 
                })
                .ToListAsync();

            return Json(ingredients);
        }

        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IngredientId,BatchQuantity,RemainingQuantity,ExpirationDate,BatchNumber,Supplier,Category")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                inventory.ReceivedDate = DateTime.UtcNow;
                if (inventory.RemainingQuantity == 0) inventory.RemainingQuantity = inventory.BatchQuantity;
                
                // Convert Supplier ID to Name if necessary
                if (int.TryParse(inventory.Supplier, out int supplierId))
                {
                    var supplier = await _context.Suppliers.FindAsync(supplierId);
                    inventory.Supplier = supplier?.Name;
                }

                // Automated Batch Number if empty
                if (string.IsNullOrEmpty(inventory.BatchNumber))
                {
                    inventory.BatchNumber = GenerateBatchNumber();
                }

                _context.Add(inventory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Stock added to inventory successfully!";
                return RedirectToAction(nameof(Index));
            }
            var fallbackList = await _context.Ingredients
                .Where(i => i.IsActive || i.Id == inventory.IngredientId)
                .OrderBy(i => i.InventoryCategory)
                .ThenBy(i => i.Name)
                .Select(i => new { Id = i.Id, DisplayName = i.Name })
                .ToListAsync();
            ViewData["IngredientId"] = new SelectList(fallbackList, "Id", "DisplayName", inventory.IngredientId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name), "Id", "Name");
            return View(inventory);
        }

        private string GenerateBatchNumber()
        {
            return $"BATCH-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        // GET: Inventory/GetEditContent/5
        public async Task<IActionResult> GetEditContent(int? id)
        {
            if (id == null) return NotFound();

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return NotFound();

            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Name == inventory.Supplier);
            var suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            var supplierList = suppliers.Select(s => new { 
                Id = s.Id, 
                DisplayName = s.Name 
            });

            var editList = await _context.Ingredients
                .Where(i => i.IsActive || i.Id == inventory.IngredientId)
                .OrderBy(i => i.InventoryCategory)
                .ThenBy(i => i.Name)
                .Select(i => new { Id = i.Id, DisplayName = i.Name })
                .ToListAsync();
            ViewData["IngredientId"] = new SelectList(editList, "Id", "DisplayName", inventory.IngredientId);
            ViewData["SupplierId"] = new SelectList(supplierList, "Id", "DisplayName", supplier?.Id);
            
            // For the dropdown to pre-select correctly, we temporarily put the ID in the Supplier property
            if (supplier != null) inventory.Supplier = supplier.Id.ToString();
            
            return PartialView("_EditPartial", inventory);
        }

        // GET: Inventory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return NotFound();

            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Name == inventory.Supplier);
            var suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            var supplierList = suppliers.Select(s => new { 
                Id = s.Id, 
                DisplayName = s.Name 
            });

            var ingredientList = await _context.Ingredients
                .Where(i => i.IsActive || i.Id == inventory.IngredientId)
                .OrderBy(i => i.InventoryCategory)
                .ThenBy(i => i.Name)
                .Select(i => new { Id = i.Id, DisplayName = i.Name })
                .ToListAsync();

            ViewData["IngredientId"] = new SelectList(ingredientList, "Id", "DisplayName", inventory.IngredientId);
            ViewData["SupplierId"] = new SelectList(supplierList, "Id", "DisplayName", supplier?.Id);
            
            if (supplier != null) inventory.Supplier = supplier.Id.ToString();

            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IngredientId,BatchQuantity,RemainingQuantity,ExpirationDate,ReceivedDate,BatchNumber,Supplier,Category")] Inventory inventory)
        {
            if (id != inventory.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Convert Supplier ID to Name if it's a numeric ID
                    if (!string.IsNullOrEmpty(inventory.Supplier) && int.TryParse(inventory.Supplier, out int sId))
                    {
                        var s = await _context.Suppliers.FindAsync(sId);
                        if (s != null) inventory.Supplier = s.Name;
                    }

                    _context.Update(inventory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Inventory record updated!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryExists(inventory.Id)) return NotFound();
                    else throw;
                }
            }
            
            // If we fall through, we need to repopulate lists
            var currentSupplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Name == inventory.Supplier);
            if (currentSupplier == null && int.TryParse(inventory.Supplier, out int sID))
            {
                currentSupplier = await _context.Suppliers.FindAsync(sID);
            }
            
            var suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            var supplierList = suppliers.Select(s => new { 
                Id = s.Id, 
                Name = s.Name 
            });

            var updateList = await _context.Ingredients
                .Where(i => i.IsActive || i.Id == inventory.IngredientId)
                .OrderBy(i => i.InventoryCategory)
                .ThenBy(i => i.Name)
                .Select(i => new { Id = i.Id, DisplayName = i.Name })
                .ToListAsync();
            ViewData["IngredientId"] = new SelectList(updateList, "Id", "DisplayName", inventory.IngredientId);
            ViewData["SupplierId"] = new SelectList(supplierList, "Id", "Name", currentSupplier?.Id);
            
            // For the dropdown to pre-select correctly, we temporarily put the ID in the Supplier property
            if (currentSupplier != null) inventory.Supplier = currentSupplier.Id.ToString();
            
            return View(inventory);
        }

        [HttpPost]
        public async Task<IActionResult> Archive(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Ingredient)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (inventory == null) return NotFound();

            // Record as wastage if it's a raw material and has quantity left
            if (inventory.Category == CrustFlowERP.Models.InventoryCategory.RawMaterials && inventory.RemainingQuantity > 0)
            {
                var wastage = new InventoryWastage
                {
                    InventoryId = inventory.Id,
                    Quantity = inventory.RemainingQuantity,
                    ReportedAt = DateTime.UtcNow,
                    Reason = "Inventory Archived",
                    Notes = $"Automatic record created during archival of batch {inventory.BatchNumber}"
                };
                
                _context.InventoryWastages.Add(wastage);
                inventory.RemainingQuantity = 0; // Stock is now considered lost/used
            }

            inventory.IsArchived = true;
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Item archived successfully. Remaining stock has been recorded as wastage.";
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventories.Any(e => e.Id == id);
        }

        // GET: Inventory/GetInventoryDetailsContent/5
        public async Task<IActionResult> GetInventoryDetailsContent(int? id)
        {
            if (id == null) return NotFound();

            var inventory = await _context.Inventories
                .Include(i => i.Ingredient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inventory == null) return NotFound();

            return PartialView("_DetailsPartial", inventory);
        }
    }
}
