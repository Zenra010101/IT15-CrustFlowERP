# 🍞 CrustFlowERP - Food Production ERP System

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue.svg)](https://docs.microsoft.com/en-us/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework%20Core-8.0-green.svg)](https://docs.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A comprehensive **Multi-Tenant Food Production ERP System** designed specifically for bakeries and food manufacturing businesses, supporting Micro, Small, and Medium enterprises with role-based access control and complete business process management.

## 🌟 Features

### 🏭 **Production Management**
- **Production Planning**: Schedule and manage production batches
- **Recipe Management**: Formulate and cost recipes with ingredient tracking
- **Quality Control**: Comprehensive quality checks and inspections
- **Wastage Tracking**: Monitor and analyze production waste
- **Efficiency Analytics**: Production performance metrics and reporting

### 📦 **Inventory Management**
- **Real-time Stock Tracking**: Monitor ingredient levels and product inventory
- **Multi-Location Support**: Warehouse and storage area management
- **ABC Classification**: Inventory categorization and optimization
- **Reorder Management**: Automated stock level alerts and replenishment
- **Stock Movements**: Complete audit trail of all inventory transactions

### 💰 **Sales & Customer Management**
- **Customer Relationship Management**: Complete customer data and history
- **Order Processing**: Streamlined sales order workflow
- **Pricing Management**: Dynamic pricing with customer-specific rates
- **Invoice Generation**: Automated billing and payment processing
- **Sales Analytics**: Revenue tracking and trend analysis

### 🛒 **Purchasing & Supplier Management**
- **Supplier Management**: Vendor relationship and performance tracking
- **Purchase Order Processing**: Complete procurement workflow
- **Goods Receiving**: Quality inspection and stock intake
- **Cost Analysis**: Purchase cost optimization and reporting
- **Supplier Evaluation**: Performance scoring and rating system

### 👥 **Human Resources Management**
- **Employee Management**: Complete employee data and records
- **Attendance Tracking**: Time and attendance monitoring
- **Payroll Processing**: Salary calculation and payment processing
- **Performance Management**: Employee evaluation and development
- **Leave Management**: Time-off requests and approvals

### 📊 **Financial Management**
- **Financial Reporting**: Comprehensive financial statements
- **Expense Management**: Cost tracking and approval workflows
- **Profit Analysis**: Revenue, cost, and profitability analysis
- **Budget Management**: Planning and budget control
- **COGS Tracking**: Cost of goods sold calculation and analysis

## 🏢 **Multi-Tenant Architecture**

### **Company Tiers**
- **Micro Company** (1-10 employees): Essential ERP functionality
- **Small Company** (11-50 employees): Enhanced features with departments
- **Medium Company** (51-250 employees): Full enterprise features

### **Data Isolation**
- Separate databases per tenant for complete data security
- Tier-specific connection strings and configurations
- Secure multi-tenant authentication and authorization

## 👥 **Role-Based Access Control**

### **User Roles**
1. **SuperAdmin**: System administration and tenant management
2. **Admin**: Business administration and user management
3. **Production Manager**: Production planning and supervision
4. **Production Staff**: Production execution and quality checks
5. **Quality Control Staff**: Quality inspection and compliance
6. **Warehouse Staff**: Inventory management and stock control
7. **Purchasing Officer**: Procurement and supplier management
8. **Cashier**: Sales processing and payment handling
9. **Sales Manager**: Customer relationship and sales strategy
10. **Accountant**: Financial management and reporting
11. **Manager**: Departmental oversight and reporting

### **Security Features**
- Hierarchical role permissions
- Departmental access isolation
- Session-based authentication
- CSRF protection
- Multi-tenant data isolation
- Secure password hashing

## 🛠️ **Technology Stack**

### **Backend**
- **.NET 8.0**: Latest .NET framework
- **ASP.NET Core MVC**: Web application framework
- **Entity Framework Core**: ORM and data access
- **SQL Server**: Database management system
- **ASP.NET Core Identity**: Authentication and authorization

### **Frontend**
- **Bootstrap 5**: Responsive UI framework
- **jQuery**: JavaScript library
- **Chart.js**: Data visualization
- **HTML5/CSS3**: Modern web standards

### **External Services**
- **PDFGate**: PDF generation service
- **Gmail SMTP**: Email communication
- **Multi-Database**: SQL Server instances per tenant

## 🗄️ **Database Schema**

### **Core Tables**
- **Users & Authentication**: User management and roles
- **Production**: Products, recipes, production orders, quality checks
- **Inventory**: Ingredients, stock levels, movements
- **Sales**: Customers, orders, invoices, payments
- **Purchasing**: Suppliers, purchase orders, receiving
- **HR**: Employees, attendance, payroll, performance
- **Financial**: Accounts, expenses, reports

### **Food Production Specific**
- **Recipe Formulation**: Ingredient quantities and costs
- **Quality Control**: Food safety and compliance checks
- **Allergen Tracking**: Customer and product allergen data
- **Shelf Life Management**: Expiration date tracking
- **Food Certifications**: Organic, Halal, Gluten-Free, Vegan

## 🚀 **Getting Started**

### **Prerequisites**
- .NET 8.0 SDK
- SQL Server 2019 or later
- Visual Studio 2022 or Visual Studio Code

### **Installation**

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/CrustFlowERP.git
   cd CrustFlowERP
   ```

2. **Configure Database**
   - Update connection strings in `appsettings.json`
   - Create databases using provided SQL scripts
   - Run database migrations

3. **Configure External Services**
   - Update PDFGate API key in `appsettings.json`
   - Configure SMTP settings for email
   - Set up multi-tenant database connections

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the System**
   - Navigate to `https://localhost:5001`
   - Login with default SuperAdmin credentials
   - Configure your business and users

### **Database Setup**

Execute the SQL scripts in order:
1. `CreateMicroCompanyDB_FoodERP.sql`
2. `CreateSmallCompanyDB_FoodERP.sql`
3. `CreateMediumCompanyDB_FoodERP.sql`

## 📁 **Project Structure**

```
CrustFlowERP/
├── Controllers/                 # API Controllers
│   ├── AccountController.cs      # Authentication
│   ├── ProductionManagerController.cs
│   ├── QualityControlController.cs
│   ├── InventoryController.cs
│   ├── SalesManagerController.cs
│   ├── PurchasingController.cs
│   ├── HRController.cs
│   └── AccountantController.cs
├── Models/                     # Data Models
│   ├── Production/              # Production models
│   ├── Inventory/              # Inventory models
│   ├── CRM/                   # Customer/Supplier models
│   ├── HR/                    # HR models
│   └── Financial/             # Financial models
├── Views/                      # MVC Views
├── Data/                       # Database Context
├── Attributes/                  # Custom Attributes
├── Services/                   # Business Services
├── DatabaseScripts/             # SQL Scripts
└── wwwroot/                    # Static Files
```

## 🔧 **Configuration**

### **appsettings.json**
```json
{
  "ConnectionStrings": {
    "CrustFlowConnectionString": "Server=...",
    "CompanyDatabases": {
      "MicroCompany": "Server=...",
      "SmallCompany": "Server=...",
      "MediumCompany": "Server=..."
    }
  },
  "PDFGate": {
    "ApiKey": "your-api-key",
    "BaseUrl": "https://api.pdfgate.com/v1/generate/pdf"
  },
  "Smtp": {
    "Server": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

## 📚 **API Documentation**

### **Authentication Endpoints**
- `POST /Account/Login` - User authentication
- `POST /Account/Logout` - User logout

### **Production Endpoints**
- `GET /ProductionManager/Index` - Production dashboard
- `POST /ProductionManager/CreateProductionOrder` - Create production order

### **Inventory Endpoints**
- `GET /Inventory/Index` - Inventory dashboard
- `POST /Inventory/UpdateStock` - Update stock levels

### **Sales Endpoints**
- `GET /SalesManager/Index` - Sales dashboard
- `GET /SalesManager/SalesReports` - Sales analytics

### **Purchasing Endpoints**
- `GET /Purchasing/Index` - Purchasing dashboard
- `GET /Purchasing/DownloadPurchaseOrder/{id}` - Download PO PDF

## 🔒 **Security**

### **Authentication**
- ASP.NET Core Identity with secure password hashing
- Multi-factor authentication support
- Session-based authentication with timeout

### **Authorization**
- Role-based access control with hierarchical permissions
- Departmental isolation
- Module-specific restrictions

### **Data Protection**
- Multi-tenant data isolation
- CSRF protection
- Input validation and sanitization
- HTTPS enforcement

## 🧪 **Testing**

### **Unit Tests**
```bash
dotnet test CrustFlowERP.Tests
```

### **Integration Tests**
```bash
dotnet test CrustFlowERP.IntegrationTests
```

## 📈 **Performance**

### **Optimization Features**
- Database connection pooling
- Caching mechanisms
- Asynchronous processing
- Pagination for large datasets
- Background processing for non-critical tasks

## 🤝 **Contributing**

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 **Support**

For support and questions:
- 📧 Email: support@crustflowerp.com
- 🐛 Issues: [GitHub Issues](https://github.com/yourusername/CrustFlowERP/issues)
- 📖 Documentation: [Wiki](https://github.com/yourusername/CrustFlowERP/wiki)

## 🙏 **Acknowledgments**

- **ASP.NET Core Team** - Excellent web framework
- **Entity Framework Team** - Powerful ORM
- **Bootstrap Team** - Responsive UI framework
- **Chart.js Team** - Data visualization library

## 🎯 **Roadmap**

### **Version 2.0**
- [ ] Mobile application
- [ ] Advanced analytics dashboard
- [ ] API for third-party integrations
- [ ] Multi-language support
- [ ] Advanced reporting features

### **Version 2.1**
- [ ] AI-powered production optimization
- [ ] Predictive inventory management
- [ ] Advanced financial analytics
- [ ] Integration with accounting software

---

**🍞 CrustFlowERP - Complete Food Production ERP Solution for Modern Bakeries! 🎊**
