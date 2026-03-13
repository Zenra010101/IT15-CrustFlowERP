using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CrustFlowERP.Data;
using CrustFlowERP.Models;

namespace CrustFlowERP.Attributes
{
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly int _requiredRole;

        public AuthorizeRoleAttribute(int requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public AuthorizeRoleAttribute(UserRole requiredRole)
        {
            _requiredRole = (int)requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var userRole = session.GetInt32("UserRole");

            // If session is missing but user is authenticated, try to re-populate
            if (!userRole.HasValue && context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userEmail = context.HttpContext.User.Identity?.Name;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                    var user = dbContext.Users.FirstOrDefault(u => u.Email == userEmail);
                    if (user != null)
                    {
                        userRole = user.Role;
                        session.SetInt32("UserRole", user.Role);
                        session.SetString("UserRoleName", user.GetRoleName());
                        session.SetString("UserEmail", user.Email ?? string.Empty);
                    }
                }
            }

            if (!userRole.HasValue)
            {
                context.Result = new RedirectToActionResult("Unauthorized", "Home", new { area = "" });
                return;
            }

            var currentRoleValue = userRole.Value;

            // 1. High-level roles are allowed access unless specifically restricted
            // Specifically exclude SuperAdmin from HR module
            var controllerName = context.ActionDescriptor.RouteValues["controller"];
            
            if (currentRoleValue == (int)UserRole.SuperAdmin)
            {
                if (controllerName == "HR")
                {
                    context.Result = new RedirectToActionResult("Unauthorized", "Home", new { area = "" });
                    return;
                }
                return; // SuperAdmin can access anything else
            }

            if (currentRoleValue == (int)UserRole.Admin || currentRoleValue == (int)UserRole.Manager)
            {
                return;
            }

            // 2. Departmental logic: Prevent lateral access between departments (Inventory/Purchasing/Sales/Production)
            // If it's a specific functional role required, we check for that exact role or hierarchical superior
            // But specifically block lateral departmental jumps.
            
            // WarehouseStaff (6) and PurchasingOfficer (7) should be isolated from each other
            if (_requiredRole == (int)UserRole.PurchasingOfficer && currentRoleValue == (int)UserRole.WarehouseStaff)
            {
                context.Result = new RedirectToActionResult("Unauthorized", "Home", new { area = "" });
                return;
            }

            if (_requiredRole == (int)UserRole.WarehouseStaff && currentRoleValue == (int)UserRole.PurchasingOfficer)
            {
                context.Result = new RedirectToActionResult("Unauthorized", "Home", new { area = "" });
                return;
            }

            // Standard hierarchy check for other roles (Production, etc)
            // Lower numerical values generally mean higher priority (e.g., Manager > Staff)
            if (currentRoleValue > _requiredRole)
            {
                context.Result = new RedirectToActionResult("Unauthorized", "Home", new { area = "" });
            }
        }
    }

    public class AuthorizeSuperAdminAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeSuperAdminAttribute() : base((int)UserRole.SuperAdmin) { } // 1
    }

    public class AuthorizeAdminAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeAdminAttribute() : base((int)UserRole.Admin) { } // 2
    }

    public class AuthorizeProductionManagerAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeProductionManagerAttribute() : base((int)UserRole.ProductionManager) { } // 3
    }

    public class AuthorizeProductionStaffAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeProductionStaffAttribute() : base((int)UserRole.ProductionStaff) { } // 4
    }

    public class AuthorizeQualityControlStaffAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeQualityControlStaffAttribute() : base((int)UserRole.QualityControlStaff) { } // 5
    }

    public class AuthorizeWarehouseStaffAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeWarehouseStaffAttribute() : base((int)UserRole.WarehouseStaff) { } // 6
    }

    public class AuthorizePurchasingOfficerAttribute : AuthorizeRoleAttribute
    {
        public AuthorizePurchasingOfficerAttribute() : base((int)UserRole.PurchasingOfficer) { } // 7
    }

    public class AuthorizeCashierAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeCashierAttribute() : base((int)UserRole.Cashier) { } // 8
    }

    public class AuthorizeSalesManagerAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeSalesManagerAttribute() : base((int)UserRole.SalesManager) { } // 9
    }

    public class AuthorizeAccountantAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeAccountantAttribute() : base((int)UserRole.Accountant) { } // 10
    }

    public class AuthorizeGeneralManagerAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeGeneralManagerAttribute() : base((int)UserRole.Manager) { } // 11
    }
}
