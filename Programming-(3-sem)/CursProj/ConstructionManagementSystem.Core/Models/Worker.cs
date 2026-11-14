namespace ConstructionManagementSystem.Models
{
    public class Worker
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public WorkerSpecialty Specialty { get; set; }
        public int BrigadeId { get; set; }
        public int? SiteId { get; set; }

        // Специфические атрибуты рабочих
        public string? QualificationLevel { get; set; }
        public DateTime? HireDate { get; set; }
        public bool HasSafetyCertification { get; set; }
    }
}