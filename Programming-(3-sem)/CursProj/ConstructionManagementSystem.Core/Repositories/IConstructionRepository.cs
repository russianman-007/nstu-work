using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Repositories
{
    public interface IConstructionRepository
    {
        // Управления
        void AddDepartment(ConstructionDepartment department);
        List<ConstructionDepartment> GetDepartments();
        ConstructionDepartment GetDepartment(int id);
        void UpdateDepartment(ConstructionDepartment department);
        void DeleteDepartment(int id);

        // Участки
        void AddSite(Site site);
        List<Site> GetSites();
        List<Site> GetSitesByDepartment(int departmentId);
        Site GetSite(int id);
        void UpdateSite(Site site);
        void DeleteSite(int id);

        // Объекты
        void AddObject(ConstructionObject obj);
        List<ConstructionObject> GetObjects();
        List<ConstructionObject> GetObjectsBySite(int siteId);
        List<ConstructionObject> GetObjectsByDepartment(int departmentId);
        ConstructionObject GetObject(int id);
        void UpdateObject(ConstructionObject obj);
        void DeleteObject(int id);

        // Сотрудники
        void AddEmployee(Employee employee);
        List<Employee> GetEmployees();
        List<Employee> GetEmployeesBySite(int siteId);
        List<Employee> GetEmployeesByDepartment(int departmentId);
        Employee GetEmployee(int id);
        void UpdateEmployee(Employee employee);
        void DeleteEmployee(int id);

        // Рабочие
        void AddWorker(Worker worker);
        List<Worker> GetWorkers();
        List<Worker> GetWorkersByBrigade(int brigadeId);
        Worker GetWorker(int id);
        void UpdateWorker(Worker worker);
        void DeleteWorker(int id);

        // Бригады
        void AddBrigade(Brigade brigade);
        List<Brigade> GetBrigades();
        List<Brigade> GetBrigadesByObject(int objectId);
        Brigade GetBrigade(int id);
        void UpdateBrigade(Brigade brigade);
        void DeleteBrigade(int id);

        // Техника
        void AddEquipment(Equipment equipment);
        List<Equipment> GetEquipment();
        List<Equipment> GetEquipmentByDepartment(int departmentId);
        List<Equipment> GetEquipmentByObject(int objectId);
        Equipment GetEquipment(int id);
        void UpdateEquipment(Equipment equipment);
        void DeleteEquipment(int id);

        // Графики работ
        void AddWorkSchedule(WorkSchedule schedule);
        List<WorkSchedule> GetWorkSchedules();
        List<WorkSchedule> GetWorkSchedulesByObject(int objectId);
        List<WorkSchedule> GetWorkSchedulesByWorkType(WorkType workType);
        List<WorkSchedule> GetDelayedSchedules();
        WorkSchedule GetWorkSchedule(int id);
        void UpdateWorkSchedule(WorkSchedule schedule);
        void DeleteWorkSchedule(int id);

        // Сметы
        void AddEstimate(Estimate estimate);
        List<Estimate> GetEstimates();
        List<Estimate> GetEstimatesByObject(int objectId);
        List<Estimate> GetEstimatesWithOverrun();
        Estimate GetEstimate(int id);
        void UpdateEstimate(Estimate estimate);
        void DeleteEstimate(int id);

        // Отчеты
        void AddWorkReport(WorkReport report);
        List<WorkReport> GetWorkReports();
        List<WorkReport> GetWorkReportsByObject(int objectId);
        List<WorkReport> GetWorkReportsByPeriod(DateTime from, DateTime to);
        WorkReport GetWorkReport(int id);
        void UpdateWorkReport(WorkReport report);
        void DeleteWorkReport(int id);

        // Заказчики
        void AddCustomer(Customer customer);
        List<Customer> GetCustomers();
        Customer GetCustomer(int id);
        void UpdateCustomer(Customer customer);
        void DeleteCustomer(int id);

        // Вспомогательные методы
        List<ConstructionObject> GetObjectsByWorkTypeAndPeriod(WorkType workType, DateTime from, DateTime to);
        decimal GetTotalBudgetByDepartment(int departmentId);
        decimal GetTotalActualCostByDepartment(int departmentId);

        void SaveData();
        void LoadData();
    }
}