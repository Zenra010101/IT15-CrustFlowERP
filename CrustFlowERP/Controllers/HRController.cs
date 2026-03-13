using CrustFlowERP.Attributes;
using CrustFlowERP.Data;
using CrustFlowERP.Models;
using CrustFlowERP.Models.HR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CrustFlowERP.Controllers
{
    [AuthorizeAdmin]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HRController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            var employeesCount = await _context.Employees.CountAsync();
            var attendanceToday = await _context.Attendances.CountAsync(a => a.Date == DateTime.Today);
            var payrollsCount = await _context.Payrolls.CountAsync(p => p.Status == "Draft");
            
            // Late Records Today (Check-in after 8:00 AM or 1:00 PM)
            var attendanceList = await _context.Attendances
                .Where(a => a.Date == DateTime.Today)
                .Select(a => new { a.CheckInMorning, a.CheckInAfternoon })
                .ToListAsync();

            var lateCount = attendanceList.Count(a => 
                (a.CheckInMorning != null && (a.CheckInMorning.Value.Hour > 8 || (a.CheckInMorning.Value.Hour == 8 && a.CheckInMorning.Value.Minute > 0))) || 
                (a.CheckInAfternoon != null && (a.CheckInAfternoon.Value.Hour > 13 || (a.CheckInAfternoon.Value.Hour == 13 && a.CheckInAfternoon.Value.Minute > 0))));

            ViewBag.EmployeesCount = employeesCount;
            ViewBag.AttendanceToday = attendanceToday;
            ViewBag.PayrollsCount = payrollsCount;
            ViewBag.LateRecords = lateCount;

            return View();
        }

        public async Task<IActionResult> Employees()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            var employees = await _context.Employees
                .Include(e => e.User)
                .ToListAsync();
            var tierStr = HttpContext.Session.GetString("CompanyTier") ?? "MicroCompany";
            ViewBag.CompanyTier = tierStr;
            ViewBag.UserLimit = tierStr switch
            {
                "MicroCompany" => 15,
                "SmallCompany" => 20,
                "MediumCompany" => 35,
                _ => 15
            };
            return View(employees);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
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
                        return Json(new { success = false, message = $"Access Denied: Your {tierStr} plan is limited to {limit} users/employees. Please upgrade your subscription to add more staff." });
                    }

                    // 1. Create the ApplicationUser account
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        EmailConfirmed = true,
                        Role = GetNumericRole(model.Position)
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // 2. Add to identity role
                        if (!await _roleManager.RoleExistsAsync(model.Position))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(model.Position));
                        }
                        await _userManager.AddToRoleAsync(user, model.Position);

                        // 3. Create the Employee record
                        var employee = new Employee
                        {
                            UserId = user.Id,
                            EmployeeCode = model.EmployeeCode,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            Position = model.Position,
                            Department = model.Department,
                            BaseSalary = model.BaseSalary,
                            DateJoined = model.DateJoined,
                            ContactNumber = model.ContactNumber,
                            Address = model.Address,
                            EmergencyContact = model.EmergencyContact,
                            EmergencyPhone = model.EmergencyPhone,
                            Status = "Active"
                        };

                        _context.Employees.Add(employee);
                        await _context.SaveChangesAsync();

                        return Json(new { success = true, message = "Employee and account created successfully" });
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        return Json(new { success = false, message = $"Failed to create account: {errors}" });
                    }
                }
                return Json(new { success = false, message = "Invalid data" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetNumericRole(string roleName)
        {
            return roleName switch
            {
                "SuperAdmin" => 1,
                "Admin" => 2,
                "Production Manager" => 3,
                "Production Staff" => 4,
                "Quality Control Staff" => 5,
                "Warehouse Staff" => 6,
                "Purchasing Officer" => 7,
                "Cashier" => 8,
                "Sales Manager" => 9,
                "Accountant" => 10,
                "Manager" => 11,
                _ => 8 // Default to Cashier
            };
        }

        public async Task<IActionResult> Attendance()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            var attendances = await _context.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Date)
                .Take(100)
                .ToListAsync();

            ViewBag.Employees = await _context.Employees.ToListAsync();
            return View(attendances);
        }

        [HttpPost]
        public async Task<IActionResult> LogAttendance([FromBody] AttendanceLogRequest model)
        {
            try
            {
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == model.EmployeeId && a.Date == DateTime.Today);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        EmployeeId = model.EmployeeId,
                        Date = DateTime.Today,
                        Status = "Present"
                    };
                    _context.Attendances.Add(attendance);
                }

                var now = DateTime.Now;
                var hour = now.Hour;

                if (model.Action == "CheckIn")
                {
                    // If before 1 PM, log as Morning In, otherwise Afternoon In
                    if (hour < 13)
                    {
                        attendance.CheckInMorning = now;
                    }
                    else
                    {
                        attendance.CheckInAfternoon = now;
                    }
                }
                else if (model.Action == "CheckOut")
                {
                    // If before 1 PM, log as Morning Out, otherwise Afternoon Out
                    if (hour < 13)
                    {
                        attendance.CheckOutMorning = now;
                    }
                    else
                    {
                        attendance.CheckOutAfternoon = now;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Attendance logged successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> GetEmployeeProfile(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Attendances)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return PartialView("_EmployeeProfilePartial", employee);
        }

        public class AttendanceLogRequest
        {
            public int EmployeeId { get; set; }
            public string Action { get; set; } = string.Empty; // CheckIn, CheckOut
        }

        public async Task<IActionResult> Payroll()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRoleName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            
            var payrolls = await _context.Payrolls
                .Include(p => p.Employee)
                .OrderByDescending(p => p.PayPeriodEnd)
                .ToListAsync();
            
            ViewBag.Employees = await _context.Employees.ToListAsync();
            return View(payrolls);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePayroll([FromBody] Payroll model)
        {
            try
            {
                _context.Payrolls.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Payroll generated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSalary([FromBody] SalaryUpdateRequest model)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(model.EmployeeId);
                if (employee == null) return Json(new { success = false, message = "Employee not found" });

                employee.BaseSalary = model.NewSalary;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Salary updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class SalaryUpdateRequest
        {
            public int EmployeeId { get; set; }
            public decimal NewSalary { get; set; }
        }
    }
}
