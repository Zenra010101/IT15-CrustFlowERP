using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CrustFlowERP.Attributes;
using CrustFlowERP.Data;
using CrustFlowERP.Models;

namespace CrustFlowERP.Controllers
{
    [AuthorizeAdmin]
    [AuthorizeGeneralManager]
    [AuthorizeCashier]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.UserRole = user.GetRoleName();
                ViewBag.UserEmail = user.Email;
                ViewBag.RoleId = user.Role;
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
