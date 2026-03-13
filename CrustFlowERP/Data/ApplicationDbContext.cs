using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CrustFlowERP.Models.Production;
using CrustFlowERP.Models.Sales;
using CrustFlowERP.Models.CRM;
using CrustFlowERP.Models.Purchasing;
using CrustFlowERP.Models.Inventory;
using CrustFlowERP.Models.HR;
using CrustFlowERP.Models;

namespace CrustFlowERP.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
    {
        // CRM Models
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }

        // Production Models
        public DbSet<Product> Products { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<ProductionLog> ProductionLogs { get; set; }
        public DbSet<QualityCheck> QualityChecks { get; set; }
        public DbSet<Wastage> Wastages { get; set; }

        // Inventory Models
        public DbSet<InventoryWastage> InventoryWastages { get; set; }

        // Sales Models
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }

        // Purchasing Models
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

        // HR Models
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }

        // Tenancy
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantPayment> TenantPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApplicationUser with Role field
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Role)
                    .HasDefaultValue(8); // Default to Cashier (8)
            });

            // Production Relationships
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Recipes)
                .HasForeignKey(r => r.ProductId);

            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Ingredient)
                .WithMany(i => i.Recipes)
                .HasForeignKey(r => r.IngredientId);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Ingredient)
                .WithMany(ing => ing.Inventories)
                .HasForeignKey(i => i.IngredientId);

            modelBuilder.Entity<ProductionOrder>()
                .HasOne(po => po.Product)
                .WithMany(p => p.ProductionOrders)
                .HasForeignKey(po => po.ProductId);

            modelBuilder.Entity<ProductionLog>()
                .HasOne(pl => pl.ProductionOrder)
                .WithMany(po => po.ProductionLogs)
                .HasForeignKey(pl => pl.ProductionOrderId);

            modelBuilder.Entity<QualityCheck>()
                .HasOne(qc => qc.ProductionOrder)
                .WithMany(po => po.QualityChecks)
                .HasForeignKey(qc => qc.ProductionOrderId);

            modelBuilder.Entity<Wastage>()
                .HasOne(w => w.ProductionOrder)
                .WithMany(po => po.Wastages)
                .HasForeignKey(w => w.ProductionOrderId);

            // Sales Relationships
            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Sale)
                .WithMany(s => s.SaleDetails)
                .HasForeignKey(sd => sd.SaleId);

            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Product)
                .WithMany()
                .HasForeignKey(sd => sd.ProductId);

            // Purchasing Relationships
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Supplier)
                .WithMany()
                .HasForeignKey(po => po.SupplierId);

            modelBuilder.Entity<PurchaseOrderDetail>()
                .HasOne(pod => pod.PurchaseOrder)
                .WithMany(po => po.PurchaseOrderDetails)
                .HasForeignKey(pod => pod.PurchaseOrderId);

            modelBuilder.Entity<PurchaseOrderDetail>()
                .HasOne(pod => pod.Ingredient)
                .WithMany()
                .HasForeignKey(pod => pod.IngredientId);

            // HR Relationships
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Employee>(e => e.UserId);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EmployeeId);

            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payrolls)
                .HasForeignKey(p => p.EmployeeId);

            // ApplicationUser Tier/Tenant Relationship
            // We skip the formal foreign key constraint if the table doesn't exist (Business DBs)
            // This prevents EF search errors and constraint violations on tenant databases
            var tenantEntity = modelBuilder.Entity<ApplicationUser>();
            
            // We use a looser mapping for TenantId in business databases
            tenantEntity.HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // If we're not in the main database, we don't want EF to manage the Tenants table
            // However, we'll keep the DbSet for the SuperAdmin's use on the main connection.
            // EF will only attempt to interact with the table if a query is explicitly made against it.
        }
    }

    public class ApplicationUser : IdentityUser
    {
        public int Role { get; set; } = 8; // Default to Cashier (8)
        
        // Multitenancy
        public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        
        // Status: Active, Deactivated, Revoked
        public string Status { get; set; } = "Active";

        public string GetRoleName()
        {
            return Role switch
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
                _ => "Unknown"
            };
        }
    }
}
