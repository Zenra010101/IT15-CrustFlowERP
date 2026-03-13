namespace CrustFlowERP.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateReceiptPdfAsync(int saleId);
        Task<byte[]> GeneratePurchaseOrderPdfAsync(int poId);
        Task<byte[]> GenerateProductionOrderPdfAsync(int poId);
    }
}
