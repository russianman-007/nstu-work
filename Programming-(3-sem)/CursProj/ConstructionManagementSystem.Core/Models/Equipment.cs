namespace ConstructionManagementSystem.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public EquipmentType Type { get; set; }
        public string InventoryNumber { get; set; }
        public int DepartmentId { get; set; }
        public int CurrentObjectId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string? Status { get; set; }          // В работе, на ремонте, свободна
        public bool IsBroken { get; set; }
    }
}