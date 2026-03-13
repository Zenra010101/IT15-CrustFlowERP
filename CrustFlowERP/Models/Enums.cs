using System.ComponentModel.DataAnnotations;

namespace CrustFlowERP.Models
{
    public static class UserRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string ProductionManager = "ProductionManager";
        public const string ProductionStaff = "ProductionStaff";
        public const string QualityControlStaff = "QualityControlStaff";
        public const string WarehouseStaff = "WarehouseStaff";
        public const string PurchasingOfficer = "PurchasingOfficer";
        public const string Cashier = "Cashier";
        public const string SalesManager = "SalesManager";
        public const string Accountant = "Accountant";
        public const string Manager = "Manager";
    }

    public enum UserRole
    {
        SuperAdmin = 1,
        Admin = 2,
        ProductionManager = 3,
        ProductionStaff = 4,
        QualityControlStaff = 5,
        WarehouseStaff = 6,
        PurchasingOfficer = 7,
        Cashier = 8,
        SalesManager = 9,
        Accountant = 10,
        Manager = 11
    }

    public enum SupplyCategory
    {
        [Display(Name = "Flour and Oats")]
        FloursAndOats = 1,
        [Display(Name = "Sugars and Sweeteners")]
        SugarsAndSweeteners = 2,
        [Display(Name = "Dairy")]
        Dairy = 3,
        [Display(Name = "Leavening Agents")]
        LeaveningAgents = 4,
        [Display(Name = "Fats and Oils")]
        FatsAndOils = 5,
        [Display(Name = "Chocolate and Cocoa")]
        ChocolateAndCocoa = 6,
        [Display(Name = "Packaging Materials")]
        PackagingMaterials = 7,
        [Display(Name = "Bakery Equipment")]
        BakeryEquipment = 8,
        [Display(Name = "Eggs")]
        Eggs = 9,
        [Display(Name = "Fruits and Nuts")]
        FruitsAndNuts = 10,
        [Display(Name = "Spices and Extracts")]
        SpicesAndExtracts = 11,
        [Display(Name = "Seeds")]
        Seeds = 12,
        [Display(Name = "Salt")]
        Salt = 13,
        [Display(Name = "Thickeners and Stabilizers")]
        ThickenersAndStabilizers = 14
    }

    public enum InventoryCategory
    {
        [Display(Name = "Raw Materials")]
        RawMaterials = 1,
        [Display(Name = "Equipment")]
        Equipment = 2,
        [Display(Name = "Packaging")]
        Packaging = 3,
        [Display(Name = "Logistics")]
        Logistics = 4,
        [Display(Name = "Services")]
        Services = 5
    }

    public enum PurchaseOrderStatus
    {
        Draft,
        Pending,
        Ordered,
        Received,
        PartiallyReceived,
        Cancelled
    }

    public enum PurchaseItemStatus
    {
        Pending,
        Received,
        Cancelled
    }

    public enum CompanyTier
    {
        MicroCompany = 1,
        SmallCompany = 2,
        MediumCompany = 3
    }

    public enum SubscriptionType
    {
        Daily = 1,
        Monthly = 2,
        Yearly = 3
    }
}
