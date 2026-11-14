namespace ConstructionManagementSystem.Models
{
    public class Estimate
    {
        public int Id { get; set; }
        public int ObjectId { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost => PlannedQuantity * UnitPrice;
        public decimal ActualCost => ActualQuantity * UnitPrice;
    }
}