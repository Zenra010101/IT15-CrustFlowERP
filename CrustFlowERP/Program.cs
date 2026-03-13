using CrustFlowERP.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using CrustFlowERP.Utilities;
using CrustFlowERP.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IStorageService, LocalStorageService>();
builder.Services.AddHttpClient<IPdfService, PdfGateService>();

builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    
    // Default connection string
    string connectionString = configuration.GetConnectionString("CrustFlowConnectionString")!;
    
    var httpContext = httpContextAccessor.HttpContext;
    if (httpContext != null)
    {
        try 
        {
            // Check session for tier (populated in AccountController.Login)
            var tier = httpContext.Session.GetString("CompanyTier");
            if (!string.IsNullOrEmpty(tier))
            {
                var tierConnectionString = configuration.GetSection("CompanyDatabases")[tier];
                if (!string.IsNullOrEmpty(tierConnectionString))
                {
                    connectionString = tierConnectionString;
                }
            }
        }
        catch
        {
            // Session not available yet, default to main DB
        }
    }
    
    options.UseSqlServer(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

// Add session support.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore/hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
