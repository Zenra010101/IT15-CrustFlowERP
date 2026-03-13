using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CrustFlowERP.Data;
using Microsoft.EntityFrameworkCore;

namespace CrustFlowERP.Services
{
    public class PdfGateService : IPdfService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public PdfGateService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
        }

        public async Task<byte[]> GenerateReceiptPdfAsync(int saleId)
        {
            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Product)
                .Include(s => s.Customer)
                .Include(s => s.Cashier)
                .FirstOrDefaultAsync(s => s.Id == saleId);

            if (sale == null) return null;

            string htmlContent = GenerateReceiptHtml(sale);
            return await CallPdfGateAsync(htmlContent, "a6");
        }

        public async Task<byte[]> GenerateProductionOrderPdfAsync(int poId)
        {
            var po = await _context.ProductionOrders
                .Include(p => p.Product)
                    .ThenInclude(prod => prod!.Recipes)
                        .ThenInclude(r => r.Ingredient)
                .FirstOrDefaultAsync(p => p.Id == poId);

            if (po == null) return null;

            string htmlContent = GenerateProductionOrderHtml(po);
            return await CallPdfGateAsync(htmlContent, "a4");
        }

        private string GenerateProductionOrderHtml(Models.Production.ProductionOrder po)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head><style>");
            sb.Append("body { font-family: 'Inter', system-ui, -apple-system, sans-serif; font-size: 14px; line-height: 1.6; margin: 40px; color: #1e293b; }");
            sb.Append(".header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 40px; border-bottom: 3px solid #000; padding-bottom: 20px; }");
            sb.Append(".shop-floor-tag { background: #000; color: #fff; padding: 5px 15px; font-weight: 900; text-transform: uppercase; font-size: 12px; margin-bottom: 10px; display: inline-block; }");
            sb.Append(".order-id { font-size: 48px; font-weight: 900; letter-spacing: -2px; line-height: 1; }");
            sb.Append(".product-hero { background: #f8fafc; padding: 30px; border-radius: 12px; margin-bottom: 30px; border: 1px solid #e2e8f0; }");
            sb.Append(".product-hero h1 { margin: 0; font-size: 32px; font-weight: 800; color: #0f172a; }");
            sb.Append(".stats-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 40px; }");
            sb.Append(".stat-box { border: 1px solid #e2e8f0; padding: 15px; border-radius: 8px; }");
            sb.Append(".stat-box label { font-size: 11px; text-transform: uppercase; color: #64748b; font-weight: 700; display: block; }");
            sb.Append(".stat-box span { font-size: 18px; font-weight: 700; color: #0f172a; }");
            sb.Append(".recipe-table { width: 100%; border-collapse: collapse; margin-bottom: 40px; }");
            sb.Append(".recipe-table th { text-align: left; padding: 12px; background: #0f172a; color: #fff; font-size: 12px; text-transform: uppercase; }");
            sb.Append(".recipe-table td { padding: 12px; border-bottom: 1px solid #e2e8f0; font-size: 16px; font-weight: 500; }");
            sb.Append(".checkbox { width: 20px; height: 20px; border: 2px solid #64748b; border-radius: 4px; display: inline-block; vertical-align: middle; margin-right: 10px; }");
            sb.Append(".notes-area { border: 2px dashed #cbd5e1; padding: 20px; border-radius: 12px; min-height: 100px; }");
            sb.Append(".footer { margin-top: 50px; font-size: 11px; color: #94a3b8; text-align: center; border-top: 1px solid #f1f5f9; padding-top: 20px; }");
            sb.Append("</style></head><body>");

            sb.Append("<div class='header'>");
            sb.Append("<div><span class='shop-floor-tag'>Production Job Sheet</span>");
            sb.Append($"<div class='order-id'>#PROD-{po.Id:D4}</div></div>");
            sb.Append("<div style='text-align: right;'>");
            sb.Append($"<p style='margin: 0; font-weight: 700;'>Scheduled: {po.ScheduledStart:MMM dd, yyyy}</p>");
            sb.Append($"<p style='margin: 0; font-size: 24px; font-weight: 900;'>{po.ScheduledStart:hh:mm tt}</p>");
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<div class='product-hero'>");
            sb.Append("<label style='font-size: 12px; text-transform: uppercase; color: #64748b; font-weight: 700;'>Target Product</label>");
            sb.Append($"<h1>{po.Product?.Name}</h1>");
            sb.Append($"<p style='margin: 0; color: #64748b;'>Category: {po.Product?.Category}</p>");
            sb.Append("</div>");

            sb.Append("<div class='stats-grid'>");
            sb.Append("<div class='stat-box'>");
            sb.Append("<label>Planned Output</label>");
            sb.Append($"<span>{po.QuantityPlanned:N0} Units</span>");
            sb.Append("</div>");
            sb.Append("<div class='stat-box'>");
            sb.Append("<label>Batch Status</label>");
            sb.Append($"<span>{po.Status}</span>");
            sb.Append("</div>");
            sb.Append("<div class='stat-box'>");
            sb.Append("<label>Signature</label>");
            sb.Append("<div style='border-bottom: 1px solid #000; height: 20px; margin-top: 5px;'></div>");
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<h3>Required Ingredients (BOM)</h3>");
            sb.Append("<table class='recipe-table'>");
            sb.Append("<thead><tr><th style='width: 40px;'></th><th>Ingredient</th><th>Unit Measurement</th><th style='text-align: right;'>Total Required</th></tr></thead>");
            sb.Append("<tbody>");
            if (po.Product?.Recipes != null)
            {
                foreach (var recipe in po.Product.Recipes)
                {
                    var total = recipe.QuantityRequired * po.QuantityPlanned;
                    sb.Append("<tr>");
                    sb.Append("<td><div class='checkbox'></div></td>");
                    sb.Append($"<td>{recipe.Ingredient?.Name}</td>");
                    sb.Append($"<td>{recipe.Ingredient?.Unit}</td>");
                    sb.Append($"<td style='text-align: right; font-weight: 800;'>{total:N2}</td>");
                    sb.Append("</tr>");
                }
            }
            sb.Append("</tbody></table>");

            sb.Append("<h3>Operational Notes</h3>");
            sb.Append("<div class='notes-area'>");
            sb.Append(string.IsNullOrEmpty(po.Notes) ? "Floor staff should record any batch deviations or quality findings here..." : po.Notes);
            sb.Append("</div>");

            sb.Append("<div class='footer'>");
            sb.Append("<p>Confidential Shop Floor Document - CrustFlow ERP Manufacturing System</p>");
            sb.Append($"<p>Generated on {DateTime.Now:MMMM dd, yyyy - HH:mm:ss}</p>");
            sb.Append("</div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }

        public async Task<byte[]> GeneratePurchaseOrderPdfAsync(int poId)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.CreatedBy)
                .Include(p => p.PurchaseOrderDetails)
                    .ThenInclude(d => d.Ingredient)
                .FirstOrDefaultAsync(p => p.Id == poId);

            if (po == null) return null;

            string htmlContent = GeneratePurchaseOrderHtml(po);
            return await CallPdfGateAsync(htmlContent, "a4");
        }

        private async Task<byte[]> CallPdfGateAsync(string htmlContent, string pageSize = "a6")
        {
            var apiKey = _configuration["PDFGate:ApiKey"]?.Trim();
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_PDFGATE_API_KEY")
            {
                throw new Exception("PDFGate API Key is missing or invalid in appsettings.json.");
            }

            var baseUrl = _configuration["PDFGate:BaseUrl"] ?? "https://api.pdfgate.com/v1/generate/pdf";

            var requestBody = new
            {
                html = htmlContent,
                pageSizeType = pageSize,
                marginTop = 10,
                marginRight = 10,
                marginBottom = 10,
                marginLeft = 10,
                printBackground = true
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"PDFGate API Error: {error}");
        }

        private string GeneratePurchaseOrderHtml(Models.Purchasing.PurchaseOrder po)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head><style>");
            sb.Append("body { font-family: 'Inter', system-ui, -apple-system, sans-serif; font-size: 14px; line-height: 1.6; margin: 40px; color: #1e293b; }");
            sb.Append(".header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 50px; border-bottom: 2px solid #f1f5f9; padding-bottom: 30px; }");
            sb.Append(".company-info h1 { margin: 0; color: #0d6efd; font-size: 28px; font-weight: 800; }");
            sb.Append(".doc-title { text-align: right; }");
            sb.Append(".doc-title h2 { margin: 0; color: #64748b; font-size: 24px; text-transform: uppercase; letter-spacing: 2px; }");
            sb.Append(".info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 40px; margin-bottom: 40px; }");
            sb.Append(".info-section h4 { margin: 0 0 10px 0; color: #64748b; text-transform: uppercase; font-size: 11px; letter-spacing: 1px; }");
            sb.Append(".info-section p { margin: 2px 0; font-weight: 600; }");
            sb.Append(".item-table { width: 100%; border-collapse: collapse; margin-bottom: 40px; }");
            sb.Append(".item-table th { background: #f8fafc; text-align: left; padding: 12px; border-bottom: 2px solid #e2e8f0; color: #64748b; font-size: 12px; text-transform: uppercase; }");
            sb.Append(".item-table td { padding: 12px; border-bottom: 1px solid #f1f5f9; }");
            sb.Append(".totals { margin-left: auto; width: 300px; border-top: 2px solid #f1f5f9; padding-top: 20px; }");
            sb.Append(".total-row { display: flex; justify-content: space-between; margin-bottom: 8px; }");
            sb.Append(".grand-total { font-weight: 800; font-size: 20px; color: #0f172a; border-top: 1px solid #e2e8f0; margin-top: 10px; padding-top: 10px; }");
            sb.Append(".footer { margin-top: 60px; padding-top: 20px; border-top: 1px solid #f1f5f9; font-size: 12px; color: #94a3b8; text-align: center; }");
            sb.Append("</style></head><body>");

            sb.Append("<div class='header'>");
            sb.Append("<div class='company-info'><h1>CRUSTFLOW ERP</h1><p>Supply Chain Logistics Unit</p></div>");
            sb.Append($"<div class='doc-title'><h2>Purchase Order</h2><p>Ref: {po.OrderNumber}</p></div>");
            sb.Append("</div>");

            sb.Append("<div class='info-grid'>");
            sb.Append("<div class='info-section'>");
            sb.Append("<h4>Supplier Information</h4>");
            sb.Append($"<p>{po.Supplier?.Name}</p>");
            sb.Append($"<p>{po.Supplier?.ContactName}</p>");
            sb.Append($"<p>{po.Supplier?.Email}</p>");
            sb.Append("</div>");
            sb.Append("<div class='info-section' style='text-align: right;'>");
            sb.Append("<h4>Order Details</h4>");
            sb.Append($"<p>Date: {po.OrderDate:MMM dd, yyyy}</p>");
            sb.Append($"<p>Expected: {(po.ExpectedDeliveryDate.HasValue ? po.ExpectedDeliveryDate.Value.ToString("MMM dd, yyyy") : "TBD")}</p>");
            sb.Append($"<p>Issued By: {po.CreatedBy?.Email ?? "System"}</p>");
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<table class='item-table'>");
            sb.Append("<thead><tr><th>Description</th><th>Quantity</th><th>Unit Price</th><th style='text-align:right;'>Total</th></tr></thead>");
            sb.Append("<tbody>");
            foreach (var detail in po.PurchaseOrderDetails)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{detail.Ingredient?.Name}</td>");
                sb.Append($"<td>{detail.Quantity} {detail.Ingredient?.Unit}</td>");
                sb.Append($"<td>₱{detail.UnitPrice:N2}</td>");
                sb.Append($"<td style='text-align:right;'>₱{detail.TotalPrice:N2}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table>");

            sb.Append("<div class='totals'>");
            sb.Append($"<div class='total-row'><span>Total Items</span><span>{po.PurchaseOrderDetails.Count}</span></div>");
            sb.Append($"<div class='total-row grand-total'><span>NET TOTAL</span><span>₱{po.TotalAmount:N2}</span></div>");
            sb.Append("</div>");

            if (!string.IsNullOrEmpty(po.Notes))
            {
                sb.Append("<div style='margin-top: 40px; padding: 20px; background: #f8fafc; border-radius: 8px;'>");
                sb.Append("<h4 style='margin: 0 0 10px 0; font-size: 11px; text-transform: uppercase;'>Special Instructions:</h4>");
                sb.Append($"<p style='margin: 0;'>{po.Notes}</p></div>");
            }

            sb.Append("<div class='footer'>");
            sb.Append("<p>This is a computer-generated document. No signature required.</p>");
            sb.Append("<p>CrustFlow ERP - Industrial Baking & Distribution System</p>");
            sb.Append("</div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }

        private string GenerateReceiptHtml(Models.Sales.Sale sale)
        {
            var cashierName = sale.Cashier?.Email ?? "SYSTEM";
            
            // Try to find the associated Employee record for a better name
            if (sale.Cashier != null)
            {
                var employee = _context.Employees.FirstOrDefault(e => e.UserId == sale.CashierId);
                if (employee != null)
                {
                    cashierName = $"{employee.FirstName} {employee.LastName}";
                }
            }

            var sb = new StringBuilder();
            sb.Append("<html><head><style>");
            sb.Append("body { font-family: 'Courier New', Courier, monospace; font-size: 12px; line-height: 1.2; margin: 0; padding: 0; color: #000; }");
            sb.Append(".header { text-align: center; margin-bottom: 10px; border-bottom: 1px dashed #000; padding-bottom: 5px; }");
            sb.Append(".info { margin-bottom: 10px; }");
            sb.Append(".item-table { width: 100%; border-collapse: collapse; margin-bottom: 10px; }");
            sb.Append(".item-table th { text-align: left; border-bottom: 1px solid #000; padding: 2px 0; }");
            sb.Append(".item-table td { padding: 2px 0; vertical-align: top; }");
            sb.Append(".totals { border-top: 1px dashed #000; padding-top: 5px; margin-top: 5px; }");
            sb.Append(".total-row { display: flex; justify-content: space-between; font-weight: bold; font-size: 14px; margin-top: 5px; }");
            sb.Append(".sub-row { display: flex; justify-content: space-between; font-size: 11px; }");
            sb.Append(".footer { text-align: center; margin-top: 20px; font-size: 10px; font-style: italic; }");
            sb.Append("</style></head><body>");

            sb.Append("<div class='header'>");
            sb.Append("<h2 style='margin:0;'>CRUSTFLOW ERP</h2>");
            sb.Append("<p style='margin:2px 0;'>Quality Bakes & Pastries</p>");
            sb.Append("</div>");

            sb.Append("<div class='info'>");
            sb.Append($"<div>RECEIPT: #SALE-{sale.Id:D4}</div>");
            sb.Append($"<div>DATE: {sale.SaleDate:yyyy-MM-dd HH:mm}</div>");
            sb.Append($"<div>CASHIER: {cashierName.ToUpper()}</div>");
            sb.Append($"<div>CUSTOMER: {sale.Customer?.Name ?? "GUEST"}</div>");
            sb.Append($"<div>PAYMENT: {sale.PaymentMethod?.ToUpper() ?? "CASH"}</div>");
            sb.Append("</div>");

            sb.Append("<table class='item-table'>");
            sb.Append("<thead><tr><th>ITEM</th><th style='text-align:center;'>QTY</th><th style='text-align:right;'>PRICE</th></tr></thead>");
            sb.Append("<tbody>");
            foreach (var detail in sale.SaleDetails)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{detail.Product?.Name ?? "Item"}</td>");
                sb.Append($"<td style='text-align:center;'>{detail.Quantity}</td>");
                sb.Append($"<td style='text-align:right;'>{detail.Subtotal:N2}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table>");

            var subtotal = sale.TotalAmount / 1.12m;
            var tax = sale.TotalAmount - subtotal;

            sb.Append("<div class='totals'>");
            sb.Append($"<div class='sub-row'><span>SUBTOTAL (EXCL VAT)</span><span>{subtotal:N2}</span></div>");
            sb.Append($"<div class='sub-row'><span>VAT (12%)</span><span>{tax:N2}</span></div>");
            sb.Append($"<div class='total-row'><span>TOTAL DUE</span><span>PHP {sale.TotalAmount:N2}</span></div>");
            sb.Append("</div>");

            sb.Append("<div class='footer'>");
            sb.Append("<p>Thank you for your purchase!</p>");
            sb.Append("<p>Please come again.</p>");
            sb.Append("</div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}
