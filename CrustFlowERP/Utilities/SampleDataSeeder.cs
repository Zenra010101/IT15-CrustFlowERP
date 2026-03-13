using Microsoft.AspNetCore.Identity;
using CrustFlowERP.Data;
using CrustFlowERP.Models.CRM;
using CrustFlowERP.Models.Production;
using CrustFlowERP.Models.Sales;
using CrustFlowERP.Models;
using CrustFlowERP.Models.HR;
using Microsoft.EntityFrameworkCore;

namespace CrustFlowERP.Utilities
{
    public static class SampleDataSeeder
    {
        public static async Task SeedAllAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // 0. Ensure we have at least one user for Sales/Production
            var users = await userManager.Users.ToListAsync();
            if (!users.Any()) return;
            var defaultUserId = users.First().Id;
            var defaultUserName = users.First().UserName ?? "System";

            // CLEAR EXISTING DATA (Except Users/Roles)
            context.SaleDetails.RemoveRange(context.SaleDetails);
            context.Sales.RemoveRange(context.Sales);
            context.Wastages.RemoveRange(context.Wastages);
            context.QualityChecks.RemoveRange(context.QualityChecks);
            context.ProductionLogs.RemoveRange(context.ProductionLogs);
            context.ProductionOrders.RemoveRange(context.ProductionOrders);
            context.Recipes.RemoveRange(context.Recipes);
            context.Inventories.RemoveRange(context.Inventories);
            context.Products.RemoveRange(context.Products);
            context.Ingredients.RemoveRange(context.Ingredients);
            context.Customers.RemoveRange(context.Customers);
            context.Suppliers.RemoveRange(context.Suppliers);
            
            // HR Models
            context.Payrolls.RemoveRange(context.Payrolls);
            context.Attendances.RemoveRange(context.Attendances);
            context.Employees.RemoveRange(context.Employees);
            
            await context.SaveChangesAsync();

            var random = new Random();

            // 1. Seed Suppliers (50) - Realistic Names
            string[] supplierNames = { 
                "Harvest Gold Flour", "Pure Cane Sugars Co.", "Elite Dairy Products", "Baker's Choice Yeast", 
                "Premium Packaging Inc", "Global Grain Distributors", "The Yeast Factory", "Farm Fresh Eggs Ltd", 
                "Sweet Horizons", "Crystal Clear Water Systems", "Bakery Equipment Pro", "Natural Butter Source", 
                "Gourmet Spices & Herbs", "Wholesale Chocolate Co", "Sanitary Supplies Plus", "Oceanic Salt Works",
                "Metro Millers", "Sunflower Oils Inc.", "Highland Grains", "Pacific Preserves", "Valley Orchards",
                "City Cold Storage", "Continental Cocoa", "Emerald Extracts", "Sunrise Dairy", "Western Wheat",
                "Oriental Spices", "Southern Sweets", "Northern Nut Co.", "Baker's Best Milling", "Gourmet Grains",
                "Flavor First Extracts", "Quality Crushing Ltd", "Standard Sugar Corp", "Essential Equipment",
                "Baking Bond", "Master Mil", "Dairy Delight", "Egg Experts", "Power Packing", "Fine Flour Ltd",
                "Sweet Sourcing", "Global Greases", "Better Butter", "Pure Yeast Pros", "Grain Guard",
                "Harvest Helpers", "Mill Masters", "Dairy Dynamics", "Sugar Specialized"
            };

            var suppliers = new List<Supplier>();
            string[] categories = { "Raw Materials", "Packaging", "Equipment", "Logistics" };
            for (int i = 0; i < 50; i++)
            {
                var supplier = new Supplier
                {
                    Name = supplierNames[i],
                    ContactName = $"Manager {i + 1}",
                    Email = $"contact@{supplierNames[i].Replace(" ", "").ToLower()}.com",
                    Phone = $"+63-915-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
                    Address = $"{random.Next(1, 400)} Business District, Metro Area",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Better categorization based on name
                string name = supplier.Name;
                if (name.Contains("Flour") || name.Contains("Mill") || name.Contains("Grain") || name.Contains("Wheat"))
                {
                    supplier.MainProductType = SupplyCategory.FloursAndOats;
                    supplier.Category = "Raw Materials";
                }
                else if (name.Contains("Sugar") || name.Contains("Sweet"))
                {
                    supplier.MainProductType = SupplyCategory.SugarsAndSweeteners;
                    supplier.Category = "Raw Materials";
                }
                else if (name.Contains("Dairy") || name.Contains("Butter") || name.Contains("Milk") || name.Contains("Cream") || name.Contains("Cold Storage"))
                {
                    supplier.MainProductType = SupplyCategory.Dairy;
                    supplier.Category = "Raw Materials";
                }
                else if (name.Contains("Yeast") || name.Contains("Baking Bond"))
                {
                    supplier.MainProductType = SupplyCategory.LeaveningAgents;
                    supplier.Category = "Raw Materials";
                }
                else if (name.Contains("Oil") || name.Contains("Greases") || name.Contains("Crushing"))
                {
                    supplier.MainProductType = SupplyCategory.FatsAndOils;
                    supplier.Category = "Raw Materials";
                }
                else if (name.Contains("Chocolate") || name.Contains("Cocoa"))
                {
                    supplier.MainProductType = SupplyCategory.ChocolateAndCocoa;
                    supplier.Category = "Raw Materials";
                }
                else if (name.Contains("Packaging") || name.Contains("Packing") || name.Contains("Supplies") || name.Contains("Guard"))
                {
                    supplier.MainProductType = SupplyCategory.PackagingMaterials;
                    supplier.Category = "Packaging";
                }
                else if (name.Contains("Equipment") || name.Contains("Tool"))
                {
                    supplier.MainProductType = SupplyCategory.BakeryEquipment;
                    supplier.Category = "Equipment";
                }
                else if (name.Contains("Egg"))
                {
                    supplier.MainProductType = SupplyCategory.Eggs;
                    supplier.Category = "Raw Materials";
                }
                else if (name.Contains("Fruit") || name.Contains("Nut") || name.Contains("Orchard") || name.Contains("Preserve"))
                {
                    supplier.MainProductType = SupplyCategory.FruitsAndNuts;
                    supplier.Category = "Raw Materials";
                }
                else if (name.Contains("Spice") || name.Contains("Extract") || name.Contains("Water") || name.Contains("Flavor") || name.Contains("Horizon"))
                {
                    supplier.MainProductType = SupplyCategory.SpicesAndExtracts;
                    supplier.Category = "Raw Materials";
                }
                else if (name.Contains("Salt"))
                {
                    supplier.MainProductType = SupplyCategory.Salt;
                    supplier.Category = "Raw Materials";
                }
                else
                {
                    // Fallback to a predictable pattern for 14 categories
                    supplier.MainProductType = (SupplyCategory)((i % 14) + 1);
                    supplier.Category = "Raw Materials";
                }

                suppliers.Add(supplier);
            }
            context.Suppliers.AddRange(suppliers);

            // 2. Seed Customers (50) - Realistic Names
            string[] customerNames = {
                "Morning Bloom Cafe", "The Corner Deli", "Metro Supermarket", "Sunnyside Pastries", 
                "Grand Hotel Manila", "University Canteen", "City General Hospital", "Riverside Bistro", 
                "The Bread Basket", "Starry Night Coffee", "Mountain Lodge", "Harbor View Resto", 
                "Daily Dose Bakery", "Neighborhood Grocery", "Corporate Plaza Pantry", "Ocean Breeze Cafe",
                "Golden Arches Bistro", "The Pastry Palace", "Downtown Diner", "Skyline Lounge",
                "Green Garden Eatery", "Silver Spoon Catering", "The Breakfast Club", "Midnight Snack",
                "The Coffee Shop", "Village Venue", "Central Station Cafe", "Main Street Market",
                "Parkside Pastries", "Bridge Way Bakers", "The Sweet Spot", "Dough Delight",
                "Crust & Crumb", "Oven Fresh Store", "Gourmet Garden", "Family Food Mart",
                "Elite Events", "Community Canteen", "The Food Hall", "Cornerstone Cafe",
                "Plaza Patisserie", "Urban Eats", "Fusion Flavors", "Savory Secrets",
                "The Dessert Bar", "Morning Magic", "Early Bird Deli", "Sunset Sweets",
                "The Baking Bin", "Daily Bread Co"
            };

            var customers = new List<Customer>();
            for (int i = 0; i < 50; i++)
            {
                customers.Add(new Customer
                {
                    Name = customerNames[i],
                    Email = $"info@{customerNames[i].Replace(" ", "").ToLower()}.com",
                    Phone = $"+63-917-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
                    Address = $"{random.Next(1, 500)} Commercial St, Main City",
                    CustomerType = i % 8 == 0 ? "Wholesale" : "Retail",
                    LoyaltyPoints = random.Next(10, 500),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-random.Next(1, 12)),
                    UpdatedAt = DateTime.UtcNow
                });
            }
            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();

            // 3. Seed Ingredients (50) - Realistic Names
            string[] ingredientNames = {
                "All-Purpose Flour", "Bread Flour", "Whole Wheat Flour", "Cake Flour", "Rye Flour",
                "Granulated Sugar", "Brown Sugar", "Powdered Sugar", "Muscovado Sugar", "Honey",
                "Active Dry Yeast", "Instant Yeast", "Sourdough Starter", "Salt", "Sea Salt",
                "Unsalted Butter", "Salted Butter", "Margarine", "Vegetable Oil", "Olive Oil",
                "Whole Milk", "Skim Milk", "Heavy Cream", "Buttermilk", "Large Eggs",
                "Vanilla Extract", "Cocoa Powder", "Dark Chocolate Chips", "Milk Chocolate", "White Chocolate",
                "Baking Powder", "Baking Soda", "Cinnamon", "Nutmeg", "Ginger Powder",
                "Walnuts", "Almonds", "Pecans", "Raisins", "Dried Cranberries",
                "Corn Syrup", "Maple Syrup", "Molasses", "Cream of Tartar", "Cornstarch",
                "Sesame Seeds", "Poppy Seeds", "Sunflower Seeds", "Oats", "Bread-crumbs"
            };

            var ingredients = new List<Ingredient>();
            var allSuppliers = await context.Suppliers.ToListAsync();
            string[] units = { "kg", "kg", "kg", "grams", "liters", "pieces", "bags" };
            for (int i = 0; i < 50; i++)
            {
                // Map index to Category for realism
                SupplyCategory cat = i switch {
                    < 5 => SupplyCategory.FloursAndOats,
                    < 10 => SupplyCategory.SugarsAndSweeteners,
                    < 13 => SupplyCategory.LeaveningAgents,
                    < 15 => SupplyCategory.Salt,
                    < 20 => SupplyCategory.FatsAndOils,
                    < 24 => SupplyCategory.Dairy,
                    < 25 => SupplyCategory.Eggs,
                    < 30 => SupplyCategory.ChocolateAndCocoa,
                    < 35 => SupplyCategory.LeaveningAgents,
                    < 40 => SupplyCategory.FruitsAndNuts,
                    < 43 => SupplyCategory.SpicesAndExtracts,
                    < 45 => SupplyCategory.ThickenersAndStabilizers,
                    _ => SupplyCategory.Seeds
                };

                ingredients.Add(new Ingredient
                {
                    Name = ingredientNames[i % ingredientNames.Length] + (i >= ingredientNames.Length ? $" (Grade {i/ingredientNames.Length + 1})" : ""),
                    Description = $"Pantry essential for bakery production",
                    Unit = units[i % units.Length],
                    MinimumStockLevel = 20.0m + i,
                    IsActive = true,
                    Category = cat,
                    InventoryCategory = InventoryCategory.RawMaterials,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // --- ADDING EQUIPMENT DATASET ---
            string[] equipmentNames = { "Industrial Oven", "Floor Mixer", "Baking Trays (Set of 10)", "Proofer Cabinet", "Bread Slicer", "Industrial Scale", "Display Chiller", "Dough Divider" };
            foreach (var name in equipmentNames)
            {
                ingredients.Add(new Ingredient
                {
                    Name = name,
                    Description = "Essential bakery production equipment",
                    Unit = "units",
                    MinimumStockLevel = 1.0m,
                    IsActive = true,
                    Category = SupplyCategory.BakeryEquipment,
                    InventoryCategory = InventoryCategory.Equipment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // --- ADDING PACKAGING DATASET ---
            string[] packagingNames = { "Bread Bags (100pcs)", "Pastry Boxes (50pcs)", "Cake Boxes (Gold)", "Sticker Labels (Roll)", "Eco-friendly Carrier Bags" };
            foreach (var name in packagingNames)
            {
                ingredients.Add(new Ingredient
                {
                    Name = name,
                    Description = "High-quality packaging for finished goods",
                    Unit = "packs",
                    MinimumStockLevel = 10.0m,
                    IsActive = true,
                    Category = SupplyCategory.PackagingMaterials,
                    InventoryCategory = InventoryCategory.Packaging,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // --- ADDING LOGISTICS DATASET ---
            string[] logisticsNames = { "Plastic Crates", "Wooden Pallets", "Hand Truck / Trolley", "Delivery Van Maintenance", "Cargo Netting" };
            foreach (var name in logisticsNames)
            {
                ingredients.Add(new Ingredient
                {
                    Name = name,
                    Description = "Items used for storage and transport",
                    Unit = "pieces",
                    MinimumStockLevel = 5.0m,
                    IsActive = true,
                    Category = SupplyCategory.BakeryEquipment, // Closest existing cat
                    InventoryCategory = InventoryCategory.Logistics,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // --- ADDING SERVICES DATASET ---
            string[] serviceNames = { "Monthly Pest Control", "Deep Cleaning Service", "Security Monitoring", "Electricity Utility", "Water Utility" };
            foreach (var name in serviceNames)
            {
                ingredients.Add(new Ingredient
                {
                    Name = name,
                    Description = "Operational service recurring costs",
                    Unit = "months",
                    MinimumStockLevel = 1.0m,
                    IsActive = true,
                    Category = SupplyCategory.BakeryEquipment, // Services don't fit perfectly in SupplyCategory
                    InventoryCategory = InventoryCategory.Services,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            context.Ingredients.AddRange(ingredients);
            await context.SaveChangesAsync();

            // 4. Seed Products (50) - Realistic Names
            string[] productNames = {
                "Classic Sourdough Loaf", "French Baguette", "Whole Wheat Pandesal", "Brioche Bun", "Japanese Milk Bread",
                "Butter Croissant", "Pain au Chocolat", "Cheese Ensaymada", "Ube Halaya Cake", "Chocolate Mousse Cake",
                "Blueberry Cheesecake", "Carrot Cake", "Red Velvet Cupcake", "Banana Walnut Bread", "Lemon Drizzle Cake",
                "Chocolate Chip Cookie", "Oatmeal Raisin Cookie", "Apple Danish", "Bacon & Cheese Roll", "Spanish Bread",
                "Monay Special", "Pandesal (10pcs)", "Cinnamon Roll", "Egg Pie Slice", "Chicken Empanada",
                "Ham & Cheese Croissant", "Multigrain Loaf", "Ciabatta Slipper", "Focaccia with Herbs", "Dinner Rolls (6pcs)",
                "Strawberry Shortcake", "Tiramisu", "Panna Cotta", "Macarons (Assorted)", "Beignets",
                "Pretzel Soft", "Garlic Breadstick", "Puff Pastry Square", "Fruit Tart", "Éclair",
                "Brownie Fudge", "Butterscotch Bar", "Crinkles Chocolate", "Ube Pandesal", "Bibingka Special",
                "Puto Bumbong", "Kutsinta (Bag)", "Ensaymada Queso", "Pork Bun", "Beef Curry Puff"
            };

            var products = new List<Product>();
            string[] prodCats = { "Bread", "Cakes", "Pastries", "Savory Snacks" };
            for (int i = 0; i < 50; i++)
            {
                products.Add(new Product
                {
                    Name = productNames[i % productNames.Length],
                    Description = $"High quality {productNames[i % productNames.Length]} made with premium ingredients.",
                    Category = prodCats[i % prodCats.Length],
                    Price = 25.0m + (random.Next(50, 500)),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            var allIngredients = await context.Ingredients.ToListAsync();
            var allProducts = await context.Products.ToListAsync();

            // 5. Seed Recipes
            if (!context.Recipes.Any())
            {
                for (int i = 0; i < allProducts.Count; i++)
                {
                    // Add 3 random ingredients per product
                    for (int j = 0; j < 3; j++) {
                        context.Recipes.Add(new Recipe
                        {
                            ProductId = allProducts[i].Id,
                            IngredientId = allIngredients[random.Next(allIngredients.Count)].Id,
                            QuantityRequired = (decimal)(random.NextDouble() * 2.5 + 0.1)
                        });
                    }
                }
            }

            // 6. Seed Inventories (50)
            if (!context.Inventories.Any())
            {
                for (int i = 0; i < allIngredients.Count; i++)
                {
                    var ing = allIngredients[i];

                    context.Inventories.Add(new Inventory
                    {
                        IngredientId = ing.Id,
                        BatchQuantity = 1000.0m,
                        RemainingQuantity = (decimal)random.Next(200, 800),
                        BatchNumber = $"LOT-{random.Next(1000, 9999)}",
                        ExpirationDate = DateTime.Now.AddDays(random.Next(30, 365)),
                        ReceivedDate = DateTime.Now.AddDays(-random.Next(1, 60)),
                        Supplier = allSuppliers[random.Next(allSuppliers.Count)].Name,
                        Category = ing.InventoryCategory
                    });
                }
            }
            await context.SaveChangesAsync();

            // 7. Seed ProductionOrders (50)
            if (!context.ProductionOrders.Any())
            {
                for (int i = 0; i < 50; i++)
                {
                    var orderDate = DateTime.Now.AddDays(-random.Next(1, 30));
                    var order = new ProductionOrder
                    {
                        ProductId = allProducts[random.Next(allProducts.Count)].Id,
                        QuantityPlanned = (decimal)random.Next(50, 200),
                        QuantityActual = (decimal)random.Next(45, 195),
                        ScheduledStart = orderDate,
                        ActualStart = orderDate.AddMinutes(30),
                        ActualEnd = orderDate.AddHours(3),
                        Status = ProductionStatus.Completed,
                        Notes = $"Daily production run {i + 1}"
                    };
                    context.ProductionOrders.Add(order);
                }
            }
            await context.SaveChangesAsync();

            var allOrders = await context.ProductionOrders.ToListAsync();

            // 8. Seed ProductionLogs, QualityChecks, Wastages
            if (!context.ProductionLogs.Any())
            {
                foreach (var order in allOrders)
                {
                    context.ProductionLogs.Add(new ProductionLog
                    {
                        ProductionOrderId = order.Id,
                        StaffId = defaultUserId,
                        ActivityDescription = "Baking and cooling monitoring completed.",
                        Timestamp = order.ActualEnd ?? DateTime.Now
                    });

                    context.QualityChecks.Add(new QualityCheck
                    {
                        ProductionOrderId = order.Id,
                        IsPassed = random.Next(10) > 0, // 90% pass rate
                        Remarks = "Appearance and taste check passed.",
                        CheckedBy = defaultUserName,
                        CheckedAt = order.ActualEnd?.AddMinutes(15) ?? DateTime.Now
                    });

                    context.Wastages.Add(new Wastage
                    {
                        ProductionOrderId = order.Id,
                        QuantityLost = (decimal)(random.NextDouble() * 5.0),
                        Reason = random.Next(2) == 0 ? "Burnt edges" : "Incorrect shape",
                        ReportedAt = order.ActualEnd ?? DateTime.Now
                    });
                }
            }
            await context.SaveChangesAsync();

            // 9. Seed Sales (50) and Details
            if (!context.Sales.Any())
            {
                string[] paymentMethods = { "Cash", "GCash", "Credit Card", "Bank Transfer" };
                for (int i = 0; i < 50; i++)
                {
                    var saleDate = DateTime.Now.AddDays(-random.Next(0, 7)).AddHours(-random.Next(1, 10));
                    var sale = new Sale
                    {
                        SaleDate = saleDate,
                        CashierId = defaultUserId,
                        PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)],
                        TotalAmount = 0 
                    };
                    context.Sales.Add(sale);
                    await context.SaveChangesAsync();

                    // Add 1-4 items per sale
                    decimal total = 0;
                    int itemsCount = random.Next(1, 5);
                    for (int j = 0; j < itemsCount; j++) {
                        var prod = allProducts[random.Next(allProducts.Count)];
                        int qty = random.Next(1, 10);
                        var detail = new SaleDetail
                        {
                            SaleId = sale.Id,
                            ProductId = prod.Id,
                            Quantity = qty,
                            UnitPrice = prod.Price,
                            Subtotal = qty * prod.Price
                        };
                        context.SaleDetails.Add(detail);
                        total += detail.Subtotal;
                    }
                    sale.TotalAmount = total;
                    await context.SaveChangesAsync();
                }
            }

            // 10. Seed Employees (10)
            if (!context.Employees.Any())
            {
                string[] firstNames = { "James", "Maria", "Robert", "Elena", "Michael", "Sarah", "David", "Jessica", "William", "Karen" };
                string[] lastNames = { "Santos", "Garcia", "Bautista", "Cruz", "Reyes", "Dela Cruz", "Torres", "Perez", "Lim", "Chan" };
                string[] departments = { "Production", "Warehouse", "Sales", "Accounting", "Management" };
                string[] positions = { "Baker", "Packer", "Clerk", "Accountant", "Manager" };

                for (int i = 0; i < 10; i++)
                {
                    var employee = new Employee
                    {
                        EmployeeCode = $"EMP-{1000 + i}",
                        FirstName = firstNames[i],
                        LastName = lastNames[i],
                        Email = $"{firstNames[i].ToLower()}.{lastNames[i].ToLower().Replace(" ", "")}@crustflow.com",
                        Department = departments[i % departments.Length],
                        Position = positions[i % positions.Length],
                        BaseSalary = 15000 + (random.Next(500, 2000) * 10),
                        DateJoined = DateTime.Now.AddMonths(-random.Next(1, 24)),
                        Status = "Active",
                        ContactNumber = $"09{random.Next(10, 99)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}"
                    };
                    context.Employees.Add(employee);
                }
                await context.SaveChangesAsync();

                // 11. Seed Attendance for these employees (last 5 days)
                var allEmployees = await context.Employees.ToListAsync();
                foreach (var emp in allEmployees)
                {
                    for (int day = 0; day < 5; day++)
                    {
                        var date = DateTime.Today.AddDays(-day);
                        if (date.DayOfWeek == DayOfWeek.Sunday) continue;

                        context.Attendances.Add(new Attendance
                        {
                            EmployeeId = emp.Id,
                            Date = date,
                            CheckInMorning = date.AddHours(7).AddMinutes(random.Next(0, 45)),
                            CheckOutMorning = date.AddHours(12),
                            CheckInAfternoon = date.AddHours(13),
                            CheckOutAfternoon = date.AddHours(17).AddMinutes(random.Next(0, 60)),
                            Status = random.Next(10) > 8 ? "Late" : "Present"
                        });
                    }
                }
                await context.SaveChangesAsync();

                // 12. Seed Payroll (last month)
                foreach (var emp in allEmployees)
                {
                    context.Payrolls.Add(new Payroll
                    {
                        EmployeeId = emp.Id,
                        PayPeriodStart = DateTime.Now.AddDays(-45),
                        PayPeriodEnd = DateTime.Now.AddDays(-15),
                        BasePay = emp.BaseSalary,
                        OvertimePay = random.Next(0, 2000),
                        Deductions = random.Next(100, 500),
                        Status = "Paid",
                        PaymentDate = DateTime.Now.AddDays(-14),
                        ReferenceNumber = $"PAY-{random.Next(100000, 999999)}"
                    });
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
