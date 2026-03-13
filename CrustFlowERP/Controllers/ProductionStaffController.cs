using CrustFlowERP.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CrustFlowERP.Controllers
{
    [AuthorizeProductionStaff]
    public class ProductionStaffController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.UserRole = "Production Staff";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            return View();
        }
    }
}
