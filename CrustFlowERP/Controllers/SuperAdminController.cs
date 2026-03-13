using CrustFlowERP.Attributes;
using CrustFlowERP.Models;
using CrustFlowERP.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CrustFlowERP.Utilities;
using CrustFlowERP.Models.Production;
using CrustFlowERP.Models.Sales;
using CrustFlowERP.Models.Purchasing;
using CrustFlowERP.Models.HR;
using Microsoft.Extensions.Configuration;

namespace CrustFlowERP.Controllers
{
    [AuthorizeSuperAdmin]
    public class SuperAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SuperAdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserRole = "SuperAdmin";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var model = new DashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalRoles = await _roleManager.Roles.CountAsync(),
                LockedUsers = await _userManager.Users.CountAsync(u => u.LockoutEnd > DateTimeOffset.Now),
                NewUsersToday = 0
            };

            // Fetch Raw Data from Database
            try {
                var salesRaw = await _context.Sales
                    .Include(s => s.Cashier)
                    .OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .ToListAsync();

                var productionRaw = await _context.ProductionOrders
                    .Include(p => p.CreatedBy)
                    .OrderByDescending(p => p.ScheduledStart)
                    .Take(5)
                    .ToListAsync();

                var purchasingRaw = await _context.PurchaseOrders
                    .Include(p => p.Supplier)
                    .Include(p => p.CreatedBy)
                    .OrderByDescending(p => p.OrderDate)
                    .Take(5)
                    .ToListAsync();

                var prodLogsRaw = await _context.ProductionLogs
                    .Include(l => l.Staff)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(5)
                    .ToListAsync();

                // Transform to SystemActivity In-Memory
                var salesList = salesRaw.Select(s => new SystemActivity {
                    User = s.Cashier?.UserName ?? "Cashier System",
                    Action = $"New Sale: ₱{s.TotalAmount:N2}",
                    Timestamp = s.SaleDate,
                    Status = "Completed",
                    BadgeClass = "badge-soft-success"
                });

                var productionList = productionRaw.Select(p => new SystemActivity {
                    User = p.CreatedBy?.UserName ?? "Production Hub",
                    Action = $"Order #{p.Id}",
                    Timestamp = p.ScheduledStart,
                    Status = p.Status.ToString(),
                    BadgeClass = "badge-soft-info"
                });

                var purchasingList = purchasingRaw.Select(p => new SystemActivity {
                    User = p.CreatedBy?.UserName ?? "Purchasing Dept",
                    Action = $"PO to {p.Supplier?.Name ?? "Unknown"}",
                    Timestamp = p.OrderDate,
                    Status = p.Status.ToString(),
                    BadgeClass = "badge-soft-warning"
                });

                var prodLogsList = prodLogsRaw.Select(l => new SystemActivity {
                    User = l.Staff?.UserName ?? "Production Staff",
                    Action = l.ActivityDescription,
                    Timestamp = l.Timestamp,
                    Status = "Logged",
                    BadgeClass = "badge-soft-success"
                });

                model.RecentActivities = salesList.Concat(productionList).Concat(purchasingList).Concat(prodLogsList)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                // In case of any database error, leave the list empty and don't crash
                ViewBag.LogError = ex.Message;
                model.RecentActivities = new List<SystemActivity>();
            }

            return View(model);
        }

        public async Task<IActionResult> Logs()
        {
            ViewBag.UserRole = "SuperAdmin";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var activities = new List<SystemActivity>();

            try {
                // Fetch all data for the full log view
                var salesRaw = await _context.Sales
                    .Include(s => s.Cashier)
                    .OrderByDescending(s => s.SaleDate)
                    .Take(50)
                    .ToListAsync();

                var productionRaw = await _context.ProductionOrders
                    .Include(p => p.CreatedBy)
                    .OrderByDescending(p => p.ScheduledStart)
                    .Take(50)
                    .ToListAsync();

                var purchasingRaw = await _context.PurchaseOrders
                    .Include(p => p.Supplier)
                    .Include(p => p.CreatedBy)
                    .OrderByDescending(p => p.OrderDate)
                    .Take(50)
                    .ToListAsync();

                var prodLogsRaw = await _context.ProductionLogs
                    .Include(l => l.Staff)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(50)
                    .ToListAsync();

                // Transform to SystemActivity
                var salesList = salesRaw.Select(s => new SystemActivity {
                    User = s.Cashier?.UserName ?? "Cashier System",
                    Action = $"New Sale: ₱{s.TotalAmount:N2}",
                    Timestamp = s.SaleDate,
                    Status = "Completed",
                    BadgeClass = "badge-soft-success"
                });

                var productionList = productionRaw.Select(p => new SystemActivity {
                    User = p.CreatedBy?.UserName ?? "Production Hub",
                    Action = $"Order #{p.Id}",
                    Timestamp = p.ScheduledStart,
                    Status = p.Status.ToString(),
                    BadgeClass = "badge-soft-info"
                });

                var purchasingList = purchasingRaw.Select(p => new SystemActivity {
                    User = p.CreatedBy?.UserName ?? "Purchasing Dept",
                    Action = $"PO to {p.Supplier?.Name ?? "Unknown"}",
                    Timestamp = p.OrderDate,
                    Status = p.Status.ToString(),
                    BadgeClass = "badge-soft-warning"
                });

                var prodLogsList = prodLogsRaw.Select(l => new SystemActivity {
                    User = l.Staff?.UserName ?? "Production Staff",
                    Action = l.ActivityDescription,
                    Timestamp = l.Timestamp,
                    Status = "Logged",
                    BadgeClass = "badge-soft-success"
                });

                activities = salesList.Concat(productionList).Concat(purchasingList).Concat(prodLogsList)
                    .OrderByDescending(a => a.Timestamp)
                    .ToList();
            }
            catch (Exception ex)
            {
                ViewBag.LogError = ex.Message;
            }

            return View(activities);
        }

        public async Task<IActionResult> Subscriptions()
        {
            ViewBag.UserRole = "SuperAdmin";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            var tenants = await _context.Tenants.OrderByDescending(t => t.TenantId).ToListAsync();
            
            // Get available database identifiers from config
            var dbConfigs = _configuration.GetSection("CompanyDatabases").GetChildren()
                .Select(x => new { Key = x.Key, Value = x.Value })
                .ToList();
            
            ViewBag.DatabaseNames = dbConfigs;
            
            return View(tenants);
        }

        public async Task<IActionResult> Payments()
        {
            ViewBag.UserRole = "SuperAdmin";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            var payments = await _context.TenantPayments
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
                
            // Fetch tenants to display names
            var tenantIds = payments.Select(p => p.TenantId).Distinct();
            var tenants = await _context.Tenants
                .Where(t => tenantIds.Contains(t.TenantId))
                .ToDictionaryAsync(t => t.TenantId, t => t.BusinessName);
                
            ViewBag.Tenants = tenants;
            
            return View(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTenant(Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                tenant.CreatedAt = DateTime.Now;
                tenant.IsPaid = false; // Require payment before activation
                tenant.IsArchived = false;

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                // Automatically create an admin user for the new tenant
                try
                {

                    // Derive domain from business name (alphanumeric only)
                    string cleanBusinessName = new string(tenant.BusinessName.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower();
                    string domain = !string.IsNullOrEmpty(cleanBusinessName) ? cleanBusinessName : "company";
                    string adminEmail = $"admin@{domain}.com";

                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail, // Set UserName to Email for easier sign-in
                        Email = adminEmail,
                        TenantId = tenant.TenantId,
                        Role = (int)UserRole.Admin,
                        EmailConfirmed = true
                    };

                    // Default password for first-time access
                    string defaultPassword = "Admin123!";
                    var result = await _userManager.CreateAsync(adminUser, defaultPassword);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                        TempData["SuccessMessage"] = $"Business '{tenant.BusinessName}' registered successfully! Sign in with: {adminEmail} (Pwd: {defaultPassword}). Note: Business is currently UNPAID.";
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        TempData["SuccessMessage"] = $"Business '{tenant.BusinessName}' registered, but failed to create admin: {errors}";
                    }
                }
                catch (Exception ex)
                {
                    TempData["SuccessMessage"] = $"Business '{tenant.BusinessName}' registered, but encountered an error creating admin: {ex.Message}";
                }

                return RedirectToAction(nameof(Subscriptions));
            }

            TempData["ErrorMessage"] = "Failed to register business. Please check the form data.";
            return RedirectToAction(nameof(Subscriptions));
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveTenant(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return Json(new { success = false, message = "Tenant not found" });

            tenant.IsArchived = !tenant.IsArchived; // Toggle
            await _context.SaveChangesAsync();
            return Json(new { success = true, isArchived = tenant.IsArchived });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return Json(new { success = false, message = "Tenant not found" });

            tenant.IsPaid = true;
            
            // Set initial due date if not already set manually
            if (!tenant.NextDueDate.HasValue)
            {
                tenant.NextDueDate = tenant.SubscriptionType switch
                {
                    SubscriptionType.Daily => DateTime.Now.AddDays(1),
                    SubscriptionType.Monthly => DateTime.Now.AddMonths(1),
                    SubscriptionType.Yearly => DateTime.Now.AddYears(1),
                    _ => DateTime.Now.AddMonths(1)
                };
            }

            // Calculate payment amount (Initial includes registration fee)
            decimal registrationFee = 2000;
            decimal tierValue = tenant.DatabaseName switch
            {
                CompanyTier.MicroCompany => 3000,
                CompanyTier.SmallCompany => 5000,
                CompanyTier.MediumCompany => 8000,
                _ => 0
            };
            decimal totalAmount = registrationFee + tierValue;

            var payment = new TenantPayment
            {
                TenantId = tenant.TenantId,
                Amount = totalAmount,
                PaymentDate = DateTime.Now,
                PaymentMethod = "Cash",
                Remarks = $"Initial registration and {tenant.SubscriptionType} subscription payment"
            };

            _context.TenantPayments.Add(payment);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RenewSubscription(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return Json(new { success = false, message = "Tenant not found" });

            // Calculate recurring payment amount (Tier value only)
            decimal tierValue = tenant.DatabaseName switch
            {
                CompanyTier.MicroCompany => 3000,
                CompanyTier.SmallCompany => 5000,
                CompanyTier.MediumCompany => 8000,
                _ => 0
            };

            // Update due date (extend from current due date or UtcNow if already expired)
            var baseDate = (tenant.NextDueDate > DateTime.Now) ? tenant.NextDueDate.Value : DateTime.Now;
            tenant.NextDueDate = tenant.SubscriptionType switch
            {
                SubscriptionType.Daily => baseDate.AddDays(1),
                SubscriptionType.Monthly => baseDate.AddMonths(1),
                SubscriptionType.Yearly => baseDate.AddYears(1),
                _ => baseDate.AddMonths(1)
            };

            var payment = new TenantPayment
            {
                TenantId = tenant.TenantId,
                Amount = tierValue,
                PaymentDate = DateTime.Now,
                PaymentMethod = "Cash",
                Remarks = $"Renewal of {tenant.SubscriptionType} subscription"
            };

            _context.TenantPayments.Add(payment);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ResetAndSeedUsers()
        {
            try
            {
                await DbSeeder.SeedUsersAsync(HttpContext.RequestServices);
                return Json(new { success = true, message = "Users database reset and seeded with 11 default accounts successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error during seeding: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetAndSeedData([FromServices] ApplicationDbContext context)
        {
            try
            {
                await SampleDataSeeder.SeedAllAsync(context, _userManager);
                return Json(new { success = true, message = "ERP Sample Data (Inventory, Ingredients, Sales) has been reset and seeded with fresh datasets including Equipment and Packaging." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error during data seeding: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SyncTenantSchemas()
        {
            var results = new List<string>();
            var dbConfigs = _configuration.GetSection("CompanyDatabases").GetChildren().ToList();

            foreach (var dbConfig in dbConfigs)
            {
                var connectionString = dbConfig.Value;
                if (string.IsNullOrEmpty(connectionString)) continue;

                try
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                    optionsBuilder.UseSqlServer(connectionString);

                    using (var context = new ApplicationDbContext(optionsBuilder.Options))
                    {
                        // Ensure AspNetUsers has TenantId and Status
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF NOT EXISTS (SELECT * FROM sys.columns 
                                           WHERE Name = 'TenantId' 
                                           AND Object_ID = Object_ID('AspNetUsers'))
                            BEGIN
                                ALTER TABLE AspNetUsers ADD TenantId int NULL;
                            END

                            IF NOT EXISTS (SELECT * FROM sys.columns 
                                           WHERE Name = 'Status' 
                                           AND Object_ID = Object_ID('AspNetUsers'))
                            BEGIN
                                ALTER TABLE AspNetUsers ADD Status nvarchar(50) NOT NULL DEFAULT 'Active';
                            END
                        ");

                        // Ensure Products table has latest columns
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'ImageUrl' AND Object_ID = Object_ID('Products'))
                                ALTER TABLE Products ADD ImageUrl nvarchar(500) NULL;
                            
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'StockQuantity' AND Object_ID = Object_ID('Products'))
                                ALTER TABLE Products ADD StockQuantity decimal(18,2) NOT NULL DEFAULT 0;
                            
                            if (!EXISTS (SELECT * FROM sys.columns WHERE Name = 'MinStockLevel' AND Object_ID = Object_ID('Products')))
                                ALTER TABLE Products ADD MinStockLevel decimal(18,2) NOT NULL DEFAULT 0;
                        ");

                        // Ensure Order tables have CreatedById for auditing
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'CreatedById' AND Object_ID = Object_ID('ProductionOrders'))
                                ALTER TABLE ProductionOrders ADD CreatedById nvarchar(450) NULL;

                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'CreatedById' AND Object_ID = Object_ID('PurchaseOrders'))
                                ALTER TABLE PurchaseOrders ADD CreatedById nvarchar(450) NULL;
                        ");

                        // Remove Tenants and TenantPayments from business databases (should only be in main DB)
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF OBJECT_ID('TenantPayments', 'U') IS NOT NULL DROP TABLE TenantPayments;
                            IF OBJECT_ID('Tenants', 'U') IS NOT NULL DROP TABLE Tenants;
                        ");

                        results.Add($"Successfully updated {dbConfig.Key}");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"Failed to update {dbConfig.Key}: {ex.Message}");
                }
            }

            return Json(new { success = true, logs = results });
        }

        public async Task<IActionResult> Users()
        {
            ViewBag.UserRole = "SuperAdmin";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var appUser = (ApplicationUser)user;
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    Roles = roles.ToList(),
                    NumericRole = appUser.Role,
                    RoleName = appUser.GetRoleName(),
                    IsLockedOut = await _userManager.IsLockedOutAsync(user),
                    LockoutEnd = user.LockoutEnd,
                    Status = appUser.Status ?? "Active"
                });
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> Roles()
        {
            ViewBag.UserRole = "SuperAdmin";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var roles = await _roleManager.Roles.ToListAsync();
            var roleViewModels = new List<RoleViewModel>();

            foreach (var role in roles)
            {
                var userCount = (await _userManager.GetUsersInRoleAsync(role.Name!)).Count;
                roleViewModels.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? "",
                    UserCount = userCount
                });
            }

            return View(roleViewModels);
        }

        [HttpGet]
        public IActionResult GetRoleDetails(string roleName)
        {
            var details = roleName switch
            {
                "SuperAdmin" => new { 
                    description = "Full system access and control. Can manage all tenants, users, and system-wide settings.",
                    permissions = new[] { 
                        new { category = "SaaS Management", items = new[] { "Register Tenants", "Configure Databases", "View Revenue" } },
                        new { category = "System", items = new[] { "View Audit Logs", "Database Backups", "Security Config" } }
                    }
                },
                "Admin" => new { 
                    description = "Tenant-level administrator. Manages local users, inventory, and branch configuration.",
                    permissions = new[] { 
                        new { category = "User Management", items = new[] { "Create Staff", "Assign Roles", "Reset Passwords" } },
                        new { category = "Operations", items = new[] { "Inventory Control", "Price Management", "Branch Reports" } }
                    }
                },
                "Manager" => new { 
                    description = "Oversees daily operations, production schedules, and employee attendance.",
                    permissions = new[] { 
                        new { category = "Operations", items = new[] { "Approve Orders", "Schedule Staff", "Inventory Requests" } }
                    }
                },
                "ProductionManager" => new { 
                    description = "Specialized role for managing production lines, recipes, and warehouse ingredients.",
                    permissions = new[] { 
                        new { category = "Production", items = new[] { "Recipe Management", "Production Planning", "Ingredient Monitoring" } }
                    }
                },
                "Accountant" => new { 
                    description = "Financial oversight, including payroll processing, COGS analysis, and profit reporting.",
                    permissions = new[] { 
                        new { category = "Finance", items = new[] { "Payroll Processing", "Expense Tracking", "COGS Analysis" } }
                    }
                },
                "Cashier" => new { 
                    description = "Front-line sales role. Handles customer transactions and basic sales reporting.",
                    permissions = new[] { 
                        new { category = "Sales", items = new[] { "POS Access", "Customer Registration", "Daily Remittance" } }
                    }
                },
                _ => new { 
                    description = "Standard system user with limited access based on assigned department.",
                    permissions = new[] { 
                        new { category = "General", items = new[] { "View Dashboard", "Update Profile" } }
                    }
                }
            };

            return Json(new { success = true, details });
        }

        public IActionResult SystemSettings()
        {
            ViewBag.UserRole = "SuperAdmin";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> LockUser([FromBody] string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(1));
                    await _userManager.UpdateAsync(user);
                    return Json(new { success = true, message = "User locked successfully" });
                }
                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> UnlockUser([FromBody] string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    await _userManager.UpdateAsync(user);
                    return Json(new { success = true, message = "User unlocked successfully" });
                }
                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return Json(new { success = true, message = "User deleted successfully" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete user" });
                    }
                }
                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // For now, just return success - email functionality can be added later
                    return Json(new { success = true, message = "Password reset functionality will be implemented soon" });
                }
                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return Json(new { success = false, message = "Email and password are required" });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "A user with this email already exists" });
                }

                // Determine RoleId and RoleName
                int roleId = (int)UserRole.Admin;
                string roleName = UserRoles.Admin;

                if (!string.IsNullOrEmpty(model.Role))
                {
                    if (int.TryParse(model.Role, out int parsedRoleId))
                    {
                        roleId = parsedRoleId;
                        roleName = roleId switch
                        {
                            1 => UserRoles.SuperAdmin,
                            2 => UserRoles.Admin,
                            3 => UserRoles.ProductionManager,
                            4 => UserRoles.ProductionStaff,
                            5 => UserRoles.QualityControlStaff,
                            6 => UserRoles.WarehouseStaff,
                            7 => UserRoles.PurchasingOfficer,
                            8 => UserRoles.Cashier,
                            9 => UserRoles.SalesManager,
                            10 => UserRoles.Accountant,
                            11 => UserRoles.Manager,
                            _ => UserRoles.Admin
                        };
                    }
                    else
                    {
                        roleName = model.Role;
                        roleId = roleName switch
                        {
                            UserRoles.SuperAdmin => (int)UserRole.SuperAdmin,
                            UserRoles.Admin => (int)UserRole.Admin,
                            UserRoles.ProductionManager => (int)UserRole.ProductionManager,
                            UserRoles.ProductionStaff => (int)UserRole.ProductionStaff,
                            UserRoles.QualityControlStaff => (int)UserRole.QualityControlStaff,
                            UserRoles.WarehouseStaff => (int)UserRole.WarehouseStaff,
                            UserRoles.PurchasingOfficer => (int)UserRole.PurchasingOfficer,
                            UserRoles.Cashier => (int)UserRole.Cashier,
                            UserRoles.SalesManager => (int)UserRole.SalesManager,
                            UserRoles.Accountant => (int)UserRole.Accountant,
                            UserRoles.Manager => (int)UserRole.Manager,
                            _ => (int)UserRole.Admin
                        };
                    }
                }

                // Create new user
                var newUser = new ApplicationUser
                {
                    UserName = model.Username ?? model.Email,
                    Email = model.Email,
                    Role = roleId,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);
                
                if (result.Succeeded)
                {
                    // Add assigned role
                    await _userManager.AddToRoleAsync(newUser, roleName);
                    
                    return Json(new { success = true, message = $"User with role {roleName} created successfully" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = $"Failed to create user: {errors}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            ViewBag.UserRole = "SuperAdmin";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var appUser = (ApplicationUser)user;

            var model = new EditUserViewModel
            {
                Id = user.Id ?? "",
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                Role = appUser.Role,
                SelectedRole = roles.FirstOrDefault() ?? "",
                IsLockedOut = await _userManager.IsLockedOutAsync(user),
                LockoutEnd = user.LockoutEnd
            };

            // Get all available roles for dropdown
            ViewBag.AllRoles = new List<SelectListItem>
            {
                new SelectListItem { Value = ((int)UserRole.SuperAdmin).ToString(), Text = "Super Admin", Selected = appUser.Role == (int)UserRole.SuperAdmin },
                new SelectListItem { Value = ((int)UserRole.Admin).ToString(), Text = "Admin", Selected = appUser.Role == (int)UserRole.Admin },
                new SelectListItem { Value = ((int)UserRole.ProductionManager).ToString(), Text = "Production Manager", Selected = appUser.Role == (int)UserRole.ProductionManager },
                new SelectListItem { Value = ((int)UserRole.ProductionStaff).ToString(), Text = "Production Staff", Selected = appUser.Role == (int)UserRole.ProductionStaff },
                new SelectListItem { Value = ((int)UserRole.QualityControlStaff).ToString(), Text = "Quality Control Staff", Selected = appUser.Role == (int)UserRole.QualityControlStaff },
                new SelectListItem { Value = ((int)UserRole.WarehouseStaff).ToString(), Text = "Warehouse Staff", Selected = appUser.Role == (int)UserRole.WarehouseStaff },
                new SelectListItem { Value = ((int)UserRole.PurchasingOfficer).ToString(), Text = "Purchasing Officer", Selected = appUser.Role == (int)UserRole.PurchasingOfficer },
                new SelectListItem { Value = ((int)UserRole.Cashier).ToString(), Text = "Cashier", Selected = appUser.Role == (int)UserRole.Cashier },
                new SelectListItem { Value = ((int)UserRole.SalesManager).ToString(), Text = "Sales Manager", Selected = appUser.Role == (int)UserRole.SalesManager },
                new SelectListItem { Value = ((int)UserRole.Accountant).ToString(), Text = "Accountant", Selected = appUser.Role == (int)UserRole.Accountant },
                new SelectListItem { Value = ((int)UserRole.Manager).ToString(), Text = "Manager", Selected = appUser.Role == (int)UserRole.Manager }
            };

            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> EditUser([FromBody] EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return Json(new { success = false, message = $"Validation errors: {string.Join(", ", errors)}" });
            }
            
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var appUser = (ApplicationUser)user;
            
            // Check if email already exists (excluding current user)
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null && existingUser.Id != model.Id)
            {
                return Json(new { success = false, message = "Email already exists" });
            }

            // Update user properties
            user.UserName = model.UserName;
            user.Email = model.Email;
            appUser.Role = model.Role;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Update user roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles.ToArray());
                
                // Add new role based on Role number
                string newRole = model.Role switch
                {
                    1 => UserRoles.SuperAdmin,
                    2 => UserRoles.Admin,
                    3 => UserRoles.ProductionManager,
                    4 => UserRoles.ProductionStaff,
                    5 => UserRoles.QualityControlStaff,
                    6 => UserRoles.WarehouseStaff,
                    7 => UserRoles.PurchasingOfficer,
                    8 => UserRoles.Cashier,
                    9 => UserRoles.SalesManager,
                    10 => UserRoles.Accountant,
                    11 => UserRoles.Manager,
                    _ => UserRoles.Cashier
                };
                await _userManager.AddToRoleAsync(user, newRole);

                return Json(new { success = true, message = "User updated successfully" });
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Failed to update user: {errors}" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> GetUserById([FromBody] GetUserByIdRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var appUser = (ApplicationUser)user;

                var userData = new
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    role = appUser.Role.ToString(),
                    isLockedOut = await _userManager.IsLockedOutAsync(user),
                    lockoutEnd = user.LockoutEnd,
                    status = appUser.Status ?? "Active"
                };

                return Json(new { success = true, user = userData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        public IActionResult UserDetails(string userId)
        {
            // TODO: Implement user details view
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser([FromBody] UserActionModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return Json(new { success = false, message = "User not found." });

            var appUser = user as ApplicationUser;
            if (appUser == null) return Json(new { success = false, message = "Invalid user type." });

            appUser.Status = "Deactivated";
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return Json(new { success = true });
            return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeUser([FromBody] UserActionModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return Json(new { success = false, message = "User not found." });

            var appUser = user as ApplicationUser;
            if (appUser == null) return Json(new { success = false, message = "Invalid user type." });

            appUser.Status = "Revoked";
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return Json(new { success = true });
            return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser([FromBody] UserActionModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return Json(new { success = false, message = "User not found." });

            var appUser = user as ApplicationUser;
            if (appUser == null) return Json(new { success = false, message = "Invalid user type." });

            appUser.Status = "Active";
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return Json(new { success = true });
            return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }
    }
}
