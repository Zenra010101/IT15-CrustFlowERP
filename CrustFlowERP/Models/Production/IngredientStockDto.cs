namespace CrustFlowERP.Models.Production
{
    public class IngredientStockDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal MinimumStockLevel { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double Stock { get; set; }
    }
}
