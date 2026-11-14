namespace ConstructionManagementSystem.Models
{
    public class Brigade
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LeaderId { get; set; }
        public int CurrentObjectId { get; set; }
        public WorkType? CurrentWorkType { get; set; }
    }
}