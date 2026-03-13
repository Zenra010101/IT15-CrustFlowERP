using System.Security.Claims;
using CrustFlowERP.Data;
using CrustFlowERP.Models;
using Microsoft.EntityFrameworkCore;

namespace CrustFlowERP.Services
{
    public interface ITenantService
    {
        string GetConnectionString();
        int? GetTenantId();
    }

    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public TenantService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public string GetConnectionString()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null || context.User?.Identity?.IsAuthenticated != true)
            {
                return _configuration.GetConnectionString("CrustFlowConnectionString")!;
            }

            // For simplicity, we can store the TenantId or Tier in claims or session during login
            // But let's look it up to be safe if it's not there
            var tenantIdString = context.User.FindFirstValue("TenantId") ?? context.Session.GetString("TenantId");
            
            if (string.IsNullOrEmpty(tenantIdString))
            {
                // If not in claims/session, we might need a one-time lookup in the main DB
                // This is slightly tricky inside the service if it's used by the DbContext itself (reentrancy)
                return _configuration.GetConnectionString("CrustFlowConnectionString")!;
            }

            if (int.TryParse(tenantIdString, out int tenantId))
            {
                // Here we need to know the tier. 
                // We'll assume the tier is also stored in session/claims for performance
                var tier = context.Session.GetString("CompanyTier");
                if (string.IsNullOrEmpty(tier))
                {
                    return _configuration.GetConnectionString("CrustFlowConnectionString")!;
                }

                return _configuration.GetSection("CompanyDatabases")[tier] ?? _configuration.GetConnectionString("CrustFlowConnectionString")!;
            }

            return _configuration.GetConnectionString("CrustFlowConnectionString")!;
        }

        public int? GetTenantId()
        {
            var context = _httpContextAccessor.HttpContext;
            var tenantIdString = context?.User?.FindFirstValue("TenantId") ?? context?.Session.GetString("TenantId");
            if (int.TryParse(tenantIdString, out int tenantId))
            {
                return tenantId;
            }
            return null;
        }
    }
}
