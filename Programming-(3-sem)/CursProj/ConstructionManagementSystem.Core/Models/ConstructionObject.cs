namespace ConstructionManagementSystem.Models
{
    public class ConstructionObject
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public ObjectType Type { get; set; }
        public int SiteId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualCost { get; set; }

        // Специфические характеристики
        public int? Floors { get; set; }              // Этажность (для жилых домов)
        public string? BuildingMaterial { get; set; } // Тип материала
        public int? ApartmentsCount { get; set; }     // Число квартир
        public string? SpanType { get; set; }         // Тип пролетного строения (для мостов)
        public decimal? Width { get; set; }           // Ширина
        public int? LanesCount { get; set; }          // Количество полос
    }
}