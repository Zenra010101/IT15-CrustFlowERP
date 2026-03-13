using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CrustFlowERP.Data;
using CrustFlowERP.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;


namespace CrustFlowERP.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                // check if the user belongs to an archived or expired tenant
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.TenantId.HasValue)
                {
                    // Ensure we check the main database for tenant status
                    var dbContext = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                    var tenant = await dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.TenantId == user.TenantId.Value);
                    
                    if (tenant != null)
                    {
                        if (tenant.IsArchived)
                        {
                            ModelState.AddModelError(string.Empty, "Access Denied: Your business account has been archived. Access is restricted.");
                            return View(model);
                        }

                        // Check for subscription expiry
                        if (tenant.NextDueDate.HasValue && tenant.NextDueDate.Value < DateTime.Now)
                        {
                            ModelState.AddModelError(string.Empty, "Access Denied: Your business subscription has expired. Please contact the Super Admin for renewal.");
                            return View(model);
                        }
                    }
                }

                // Check user account status (Deactivated or Revoked)
                if (user != null)
                {
                    if (string.Equals(user.Status, "Deactivated", StringComparison.OrdinalIgnoreCase) || 
                        string.Equals(user.Status, "Revoked", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, "Access Denied: Your account has been " + user.Status.ToLower() + ". Please contact support for more information.");
                        return View(model);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    if (user != null)
                    {
                        // Store role in session
                        HttpContext.Session.SetString("UserId", user.Id);
                        HttpContext.Session.SetInt32("UserRole", user.Role);
                        HttpContext.Session.SetString("UserRoleName", user.GetRoleName());
                        HttpContext.Session.SetString("UserEmail", user.Email ?? string.Empty);

                        // Multitenancy: Store TenantId and Tier
                        if (user.TenantId.HasValue)
                        {
                            HttpContext.Session.SetString("TenantId", user.TenantId.Value.ToString());
                            
                            // Get the tier name (Micro, Small, Medium) from the tenant
                            var dbContext = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                            var tenant = await dbContext.Tenants.FindAsync(user.TenantId.Value);
                            if (tenant != null)
                            {
                                HttpContext.Session.SetString("CompanyTier", tenant.DatabaseName.ToString());
                            }
                        }

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToRoleDashboard(user.Role);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "User account not found.");
                    }
                }
                else
                {
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "User with this email does not exist.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid password. Please try again.");
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private IActionResult RedirectToRoleDashboard(int role)
        {
            return role switch
            {
                1 => RedirectToAction("Index", "SuperAdmin"),
                2 => RedirectToAction("Index", "Admin"),
                3 => RedirectToAction("Index", "ProductionManager"),
                4 => RedirectToAction("Index", "ProductionStaff"),
                5 => RedirectToAction("Index", "QualityControl"),
                6 => RedirectToAction("Index", "Warehouse"),
                7 => RedirectToAction("Index", "Purchasing"),
                8 => RedirectToAction("Index", "Cashier"),
                9 => RedirectToAction("Index", "SalesManager"),
                10 => RedirectToAction("Index", "Accountant"),
                11 => RedirectToAction("Index", "Manager"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
