using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrustFlowERP.Data;
using CrustFlowERP.Models.Production;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;

namespace CrustFlowERP.Controllers
{
    [Authorize] // Generic authorize, we can add role-based checks later
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Recipes
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Recipes)
                .ThenInclude(r => r.Ingredient)
                .ToListAsync();

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserRole = "Admin"; // For layout purposes

            return View(products);
        }

        // GET: Recipes/Manage/5
        public async Task<IActionResult> Manage(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Recipes)
                .ThenInclude(r => r.Ingredient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            ViewBag.Ingredients = await _context.Ingredients.Where(i => i.IsActive).ToListAsync();
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserRole = "Admin";

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddIngredient(int productId, int ingredientId, decimal quantity)
        {
            var recipe = new Recipe
            {
                ProductId = productId,
                IngredientId = ingredientId,
                QuantityRequired = quantity
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id = productId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveIngredient(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                int productId = recipe.ProductId;
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage), new { id = productId });
            }
            return NotFound();
        }
    }
}
