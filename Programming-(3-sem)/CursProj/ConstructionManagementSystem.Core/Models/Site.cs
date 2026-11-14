namespace ConstructionManagementSystem.Models
{
    public class Site
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        public int SiteManagerId { get; set; }
        public string Address { get; set; }
    }
}