using CrustFlowERP.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CrustFlowERP.Controllers
{
    [AuthorizeWarehouseStaff]
    public class WarehouseController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.UserRole = "Warehouse Staff";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            return View();
        }
    }
}
