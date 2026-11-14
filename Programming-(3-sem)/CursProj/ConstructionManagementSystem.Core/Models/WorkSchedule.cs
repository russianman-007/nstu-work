namespace ConstructionManagementSystem.Models
{
    public class WorkSchedule
    {
        public int Id { get; set; }
        public int ObjectId { get; set; }
        public WorkType WorkType { get; set; }
        public int BrigadeId { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public WorkStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}