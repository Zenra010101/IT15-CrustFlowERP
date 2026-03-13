using CrustFlowERP.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CrustFlowERP.Controllers
{
    [AuthorizeAccountant]
    public class AccountantController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.UserRole = "Accountant";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            return View();
        }

        public IActionResult COGS()
        {
            return View();
        }

        public IActionResult Payroll()
        {
            return View();
        }

        public IActionResult ExpensesProfits()
        {
            return View();
        }

        public IActionResult FinancialReports()
        {
            return View();
        }
    }
}
