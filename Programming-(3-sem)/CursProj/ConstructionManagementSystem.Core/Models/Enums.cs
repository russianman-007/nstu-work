namespace ConstructionManagementSystem.Models
{
    // Типы объектов
    public enum ObjectType
    {
        ResidentialBuilding,
        Hospital,
        School,
        Bridge,
        Road
    }

    // Должности ИТР
    public enum Position
    {
        Engineer,
        Technologist,
        Technician,
        Foreman,
        Master,
        SiteManager,
        DepartmentHead
    }

    // Специальности рабочих
    public enum WorkerSpecialty
    {
        Mason,
        ConcreteWorker,
        Finisher,
        Welder,
        Electrician,
        Driver,
        Mechanic
    }

    // Типы техники
    public enum EquipmentType
    {
        Crane,
        Excavator,
        Bulldozer,
        PileDriver,
        Truck,
        DumpTruck,
        ConcreteMixer
    }

    // Типы работ
    public enum WorkType
    {
        Foundation,
        Brickwork,
        Plumbing,
        Electrical,
        Finishing,
        Roadwork
    }

    // Статус выполнения работ
    public enum WorkStatus
    {
        Planned,
        InProgress,
        Completed,
        Delayed
    }
}