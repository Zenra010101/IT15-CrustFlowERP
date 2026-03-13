using CrustFlowERP.Attributes;
using CrustFlowERP.Models;
using CrustFlowERP.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CrustFlowERP.Services;
using System.Web;

namespace CrustFlowERP.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender; 

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] UserActionModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null) return Json(new { success = false, message = "User not found" });

                // Generate token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // Construct the reset link (pointing to an endpoint we will define)
                var resetLink = Url.Action("ResetPassword", "Account", 
                    new { userId = user.Id, token = token }, Request.Scheme);

                // Send email via the Email API (Service)
                var emailBody = $@"
                    <h2>CrustFlowERP Password Reset</h2>
                    <p>An administrator has requested a password reset for your account.</p>
                    <p>Click the link below to set your new password:</p>
                    <a href='{resetLink}' style='padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Reset My Password</a>
                    <p>If the button above doesn't work, copy and paste this link:</p>
                    <p>{resetLink}</p>
                    <p>This link will expire in 2 hours.</p>";

                await _emailSender.SendEmailAsync(user.Email!, "Reset Your CrustFlowERP Password", emailBody);

                return Json(new { success = true, message = "Password reset link has been sent to the user's email via API." });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += " | Inner: " + ex.InnerException.Message;
                }
                return Json(new { success = false, message = "Failed to send reset email: " + errorMsg });
            }
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TodayOrdersCount = await _context.Sales.CountAsync(s => s.SaleDate.Date == today),
                TodayRevenue = await _context.Sales.Where(s => s.SaleDate.Date == today).SumAsync(s => (decimal?)s.TotalAmount) ?? 0,
                LowStockItems = (await _context.Ingredients
                    .Select(i => new { 
                        i.MinimumStockLevel, 
                        Stock = _context.Inventories
                            .Where(inv => inv.IngredientId == i.Id)
                            .Sum(inv => (decimal?)inv.RemainingQuantity) ?? 0 
                    })
                    .ToListAsync())
                    .Count(x => x.MinimumStockLevel > x.Stock),
                RecentSales = await _context.Sales
                    .Include(s => s.Cashier)
                    .OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .ToListAsync()
            };

            ViewBag.UserRole = "Admin";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            return View(model);
        }
        
        public async Task<IActionResult> UserManagement()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var appUser = (ApplicationUser)user;
                
                // Fetch full name from Employee record if available
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
                
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    Roles = roles.ToList(),
                    NumericRole = appUser.Role,
                    RoleName = appUser.GetRoleName(),
                    FullName = employee?.FullName ?? "",
                    IsLockedOut = await _userManager.IsLockedOutAsync(user),
                    LockoutEnd = user.LockoutEnd
                });
            }

            var tierStr = HttpContext.Session.GetString("CompanyTier") ?? "MicroCompany";
            ViewBag.CompanyTier = tierStr;
            ViewBag.UserLimit = tierStr switch
            {
                "MicroCompany" => 15,
                "SmallCompany" => 20,
                "MediumCompany" => 35,
                _ => 15
            };

            return View(userViewModels);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateAdminModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return Json(new { success = false, message = "Email and password are required" });
                }

                // Check user limit based on company tier
                var tierStr = HttpContext.Session.GetString("CompanyTier");
                int limit = tierStr switch
                {
                    "MicroCompany" => 15,
                    "SmallCompany" => 20,
                    "MediumCompany" => 35,
                    _ => 15 // Default to Micro if unknown
                };

                var userCount = await _userManager.Users.CountAsync();
                if (userCount >= limit)
                {
                    return Json(new { success = false, message = $"Access Denied: Your {tierStr} plan is limited to {limit} users. Please upgrade your subscription to add more staff." });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "A user with this email already exists" });
                }

                // Super admins should not be created via this endpoint by regular Admins
                if (model.Role == "SuperAdmin")
                {
                    return Json(new { success = false, message = "Cannot create SuperAdmin accounts" });
                }

                // Create new user
                var newUser = new ApplicationUser
                {
                    UserName = model.Email, // Use email as username
                    Email = model.Email,
                    Role = GetNumericRole(model.Role), // Use numeric mapping
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);
                
                if (result.Succeeded)
                {
                    // Add role to Identity system
                    var internalRoleName = model.Role.Replace(" ", ""); // Remove spaces for Identity
                    await _userManager.AddToRoleAsync(newUser, internalRoleName);
                    
                    // Create Employee record if FullName provided
                    if (!string.IsNullOrEmpty(model.FullName))
                    {
                        var names = model.FullName.Split(' ', 2);
                        var firstName = names[0];
                        var lastName = names.Length > 1 ? names[1] : "";

                        var employee = new CrustFlowERP.Models.HR.Employee
                        {
                            UserId = newUser.Id,
                            FirstName = firstName,
                            LastName = lastName,
                            Email = model.Email,
                            EmployeeCode = "EMP" + Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper()
                        };
                        _context.Employees.Add(employee);
                        await _context.SaveChangesAsync();
                    }

                    return Json(new { success = true, message = "User created successfully" });
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
        
        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Check if trying to update SuperAdmin (Admin can manage everyone else)
                var appUser = (ApplicationUser)user;
                if (appUser.Role == 1) // 1 = SuperAdmin
                {
                    return Json(new { success = false, message = "Cannot modify SuperAdmin accounts" });
                }

                // Admin should not be able to elevate a user to SuperAdmin
                if (model.Role == "SuperAdmin")
                {
                    return Json(new { success = false, message = "Cannot assign SuperAdmin role" });
                }

                user.Email = model.Email;
                user.UserName = model.Email; // Keep username in sync with email
                
                var appUserToUpdate = (ApplicationUser)user;
                appUserToUpdate.Role = GetNumericRole(model.Role); // Use numeric mapping

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    // Update role in Identity system
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles.ToArray());
                    
                    var internalRoleName = model.Role.Replace(" ", ""); // Remove spaces for Identity
                    await _userManager.AddToRoleAsync(user, internalRoleName);
                    
                    // Update or Create Employee record
                    var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
                    if (!string.IsNullOrEmpty(model.FullName))
                    {
                        var names = model.FullName.Split(' ', 2);
                        var firstName = names[0];
                        var lastName = names.Length > 1 ? names[1] : "";

                        if (employee != null)
                        {
                            employee.FirstName = firstName;
                            employee.LastName = lastName;
                            employee.Email = model.Email;
                        }
                        else
                        {
                            employee = new CrustFlowERP.Models.HR.Employee
                            {
                                UserId = user.Id,
                                FirstName = firstName,
                                LastName = lastName,
                                Email = model.Email,
                                EmployeeCode = "EMP" + Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper()
                            };
                            _context.Employees.Add(employee);
                        }
                        await _context.SaveChangesAsync();
                    }
                    
                    return Json(new { success = true, message = "User updated successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update user" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null) return Json(new { success = false, message = "User not found" });

                // SuperAdmin protection
                var appUser = (ApplicationUser)user;
                if (appUser.Role == 1) return Json(new { success = false, message = "Cannot modify SuperAdmin passwords" });

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (result.Succeeded)
                    return Json(new { success = true, message = "Password updated successfully" });
                
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Failed to update password: {errors}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DeactivateUser([FromBody] UserActionModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Check if trying to deactivate SuperAdmin
                var appUser = (ApplicationUser)user;
                if (appUser.Role == 1) // 1 = SuperAdmin
                {
                    return Json(new { success = false, message = "Cannot deactivate SuperAdmin account" });
                }

                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                await _userManager.UpdateAsync(user);
                
                return Json(new { success = true, message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> ActivateUser([FromBody] UserActionModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Check if trying to activate SuperAdmin
                var appUser = (ApplicationUser)user;
                if (appUser.Role == 1) // 1 = SuperAdmin
                {
                    return Json(new { success = false, message = "Cannot modify SuperAdmin account" });
                }

                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.UpdateAsync(user);
                
                return Json(new { success = true, message = "User activated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> ResetUserPassword([FromBody] UserActionModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Check if trying to reset SuperAdmin password
                var appUser = (ApplicationUser)user;
                if (appUser.Role == 1) // 1 = SuperAdmin
                {
                    return Json(new { success = false, message = "Cannot reset SuperAdmin password" });
                }

                // Generate new temporary password
                var newPassword = "Crust" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper() + "!";
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                
                if (result.Succeeded)
                {
                    // In a real app, you would send an email here.
                    // For now, we'll return the new password in the message so the admin can give it to the user.
                    return Json(new { success = true, message = $"Password has been reset. New temporary password: {newPassword}", tempPassword = newPassword });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to reset password" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetNumericRole(string roleName)
        {
            // Normalize role name by removing spaces if it's coming from Identity strings
            return roleName.Replace(" ", "") switch
            {
                "SuperAdmin" => 1,
                "Admin" => 2,
                "ProductionManager" => 3,
                "ProductionStaff" => 4,
                "QualityControlStaff" => 5,
                "WarehouseStaff" => 6,
                "PurchasingOfficer" => 7,
                "Cashier" => 8,
                "SalesManager" => 9,
                "Accountant" => 10,
                "Manager" => 11,
                _ => 8 // Default to Cashier
            };
        }
        
        public IActionResult Reports()
        {
            return View();
        }
        

        public IActionResult Settings()
        {
            return View();
        }
    }
}
