namespace ConstructionManagementSystem.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Position Position { get; set; }
        public int? SiteId { get; set; }              // null для управления
        public bool IsBrigadeLeader { get; set; }

        // Специфические атрибуты ИТР
        public string? Education { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Certification { get; set; }
    }
}