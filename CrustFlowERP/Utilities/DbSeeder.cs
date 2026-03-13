using Microsoft.AspNetCore.Identity;
using CrustFlowERP.Data;
using CrustFlowERP.Models;
using Microsoft.EntityFrameworkCore;

namespace CrustFlowERP.Utilities
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Clear existing users and relevant data
            // We need to clear AspNetUserRoles first because of FK constraints
            await context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserRoles");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUsers");

            // 2. Ensure Roles exist
            string[] roles = { 
                UserRoles.SuperAdmin, UserRoles.Admin, UserRoles.ProductionManager, 
                UserRoles.ProductionStaff, UserRoles.QualityControlStaff, 
                UserRoles.WarehouseStaff, UserRoles.PurchasingOfficer, 
                UserRoles.Cashier, UserRoles.SalesManager, UserRoles.Accountant, 
                UserRoles.Manager 
            };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 3. Define the new user set
            var newUsers = new List<(string Email, string UserName, int RoleId, string RoleName)>
            {
                ("superadmin@crustflow.com", "superadmin", (int)UserRole.SuperAdmin, UserRoles.SuperAdmin),
                ("admin@crustflow.com", "admin_user", (int)UserRole.Admin, UserRoles.Admin),
                ("manager@crustflow.com", "general_manager", (int)UserRole.Manager, UserRoles.Manager),
                ("prod_manager@crustflow.com", "production_boss", (int)UserRole.ProductionManager, UserRoles.ProductionManager),
                ("prod_staff@crustflow.com", "baker_01", (int)UserRole.ProductionStaff, UserRoles.ProductionStaff),
                ("qc@crustflow.com", "quality_checker", (int)UserRole.QualityControlStaff, UserRoles.QualityControlStaff),
                ("warehouse@crustflow.com", "inventory_clerk", (int)UserRole.WarehouseStaff, UserRoles.WarehouseStaff),
                ("purchasing@crustflow.com", "buyer", (int)UserRole.PurchasingOfficer, UserRoles.PurchasingOfficer),
                ("cashier@crustflow.com", "cashier_01", (int)UserRole.Cashier, UserRoles.Cashier),
                ("sales@crustflow.com", "sales_head", (int)UserRole.SalesManager, UserRoles.SalesManager),
                ("accountant@crustflow.com", "bookkeeper", (int)UserRole.Accountant, UserRoles.Accountant)
            };

            string defaultPassword = "P@ssword123";

            foreach (var u in newUsers)
            {
                var user = new ApplicationUser
                {
                    UserName = u.Email, // Set UserName to Email for easier sign-in
                    Email = u.Email,
                    EmailConfirmed = true,
                    Role = u.RoleId
                };

                var result = await userManager.CreateAsync(user, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, u.RoleName);
                }
            }
        }
    }
}
