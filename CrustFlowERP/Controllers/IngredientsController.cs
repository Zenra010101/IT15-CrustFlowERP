using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrustFlowERP.Data;
using CrustFlowERP.Models.Production;
using CrustFlowERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CrustFlowERP.Controllers
{
    [Authorize]
    public class IngredientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IngredientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ingredients
        public async Task<IActionResult> Index()
        {
            // Data Fix: Set all Ingredients with category 0 (unset) to RawMaterials (1)
            var legacyIngredients = await _context.Ingredients
                .Where(i => (int)i.InventoryCategory == 0)
                .ToListAsync();
            if (legacyIngredients.Any())
            {
                foreach (var i in legacyIngredients) i.InventoryCategory = CrustFlowERP.Models.InventoryCategory.RawMaterials;
                await _context.SaveChangesAsync();
            }

            // Data Fix: Set all Inventory records with category 0 (unset) to RawMaterials (1)
            var legacyInventory = await _context.Inventories
                .Where(i => (int)i.Category == 0)
                .ToListAsync();
            if (legacyInventory.Any())
            {
                foreach (var i in legacyInventory) i.Category = CrustFlowERP.Models.InventoryCategory.RawMaterials;
                await _context.SaveChangesAsync();
            }

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserRole = "Admin";
            var ingredients = await _context.Ingredients
                .Include(i => i.Inventories)
                .Where(i => i.InventoryCategory == InventoryCategory.RawMaterials)
                .OrderBy(i => i.Name)
                .ToListAsync();
            return View(ingredients);
        }

        // GET: Ingredients/Create
        public IActionResult Create()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserRole = "Admin";
            return View();
        }

        // POST: Ingredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Unit,IsActive,MinimumStockLevel,Category,InventoryCategory")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                ingredient.CreatedAt = DateTime.UtcNow;
                ingredient.UpdatedAt = DateTime.UtcNow;
                _context.Add(ingredient);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ingredient created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(ingredient);
        }

        // GET: Ingredients/GetEditContent/5
        public async Task<IActionResult> GetEditContent(int? id)
        {
            if (id == null) return NotFound();

            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return NotFound();

            return PartialView("_EditPartial", ingredient);
        }

        // GET: Ingredients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return NotFound();
            
            return View(ingredient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Unit,IsActive,MinimumStockLevel,Category,InventoryCategory,CreatedAt")] Ingredient ingredient)
        {
            if (id != ingredient.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    ingredient.UpdatedAt = DateTime.UtcNow;
                    _context.Update(ingredient);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ingredient updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IngredientExists(ingredient.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ingredient);
        }

        private bool IngredientExists(int id)
        {
            return _context.Ingredients.Any(e => e.Id == id);
        }
    }
}
