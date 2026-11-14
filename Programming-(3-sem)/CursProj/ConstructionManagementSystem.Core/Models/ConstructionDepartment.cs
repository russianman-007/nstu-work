namespace ConstructionManagementSystem.Models
{
    public class ConstructionDepartment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int DepartmentHeadId { get; set; }
    }
}