namespace ConstructionManagementSystem.Models
{
    public class WorkReport
    {
        public int Id { get; set; }
        public int WorkScheduleId { get; set; }
        public DateTime ReportDate { get; set; }
        public int CompletedPercentage { get; set; }
        public string? Description { get; set; }
        public decimal? AdditionalCosts { get; set; }
        public string? Issues { get; set; }
        public int ReportedBy { get; set; }           // Кто составил отчет
    }
}