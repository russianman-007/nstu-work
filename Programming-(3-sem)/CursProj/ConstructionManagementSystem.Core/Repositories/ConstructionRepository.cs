using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Repositories
{
    public class ConstructionRepository : IConstructionRepository
    {
        private List<ConstructionDepartment> departments = new List<ConstructionDepartment>();
        private List<Site> sites = new List<Site>();
        private List<ConstructionObject> objects = new List<ConstructionObject>();
        private List<Employee> employees = new List<Employee>();
        private List<Worker> workers = new List<Worker>();
        private List<Brigade> brigades = new List<Brigade>();
        private List<Equipment> equipment = new List<Equipment>();
        private List<WorkSchedule> workSchedules = new List<WorkSchedule>();
        private List<Estimate> estimates = new List<Estimate>();
        private List<WorkReport> workReports = new List<WorkReport>();
        private List<Customer> customers = new List<Customer>();

        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ConstructionRepository()
        {
            LoadData();
        }

        public string GetDataDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dataDir = Path.Combine(appData, "ConstructionManagementSystem");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            return dataDir;
        }

        public void SaveData()
        {
            try
            {
                var dataDir = GetDataDirectory();
                SaveToFile(departments, Path.Combine(dataDir, "departments.json"));
                SaveToFile(sites, Path.Combine(dataDir, "sites.json"));
                SaveToFile(objects, Path.Combine(dataDir, "objects.json"));
                SaveToFile(employees, Path.Combine(dataDir, "employees.json"));
                SaveToFile(workers, Path.Combine(dataDir, "workers.json"));
                SaveToFile(brigades, Path.Combine(dataDir, "brigades.json"));
                SaveToFile(equipment, Path.Combine(dataDir, "equipment.json"));
                SaveToFile(workSchedules, Path.Combine(dataDir, "workschedules.json"));
                SaveToFile(estimates, Path.Combine(dataDir, "estimates.json"));
                SaveToFile(workReports, Path.Combine(dataDir, "workreports.json"));
                SaveToFile(customers, Path.Combine(dataDir, "customers.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
            }
        }

        public void LoadData()
        {
            try
            {
                var dataDir = GetDataDirectory();
                departments = LoadFromFile<List<ConstructionDepartment>>(Path.Combine(dataDir, "departments.json")) ?? new List<ConstructionDepartment>();
                sites = LoadFromFile<List<Site>>(Path.Combine(dataDir, "sites.json")) ?? new List<Site>();
                objects = LoadFromFile<List<ConstructionObject>>(Path.Combine(dataDir, "objects.json")) ?? new List<ConstructionObject>();
                employees = LoadFromFile<List<Employee>>(Path.Combine(dataDir, "employees.json")) ?? new List<Employee>();
                workers = LoadFromFile<List<Worker>>(Path.Combine(dataDir, "workers.json")) ?? new List<Worker>();
                brigades = LoadFromFile<List<Brigade>>(Path.Combine(dataDir, "brigades.json")) ?? new List<Brigade>();
                equipment = LoadFromFile<List<Equipment>>(Path.Combine(dataDir, "equipment.json")) ?? new List<Equipment>();
                workSchedules = LoadFromFile<List<WorkSchedule>>(Path.Combine(dataDir, "workschedules.json")) ?? new List<WorkSchedule>();
                estimates = LoadFromFile<List<Estimate>>(Path.Combine(dataDir, "estimates.json")) ?? new List<Estimate>();
                workReports = LoadFromFile<List<WorkReport>>(Path.Combine(dataDir, "workreports.json")) ?? new List<WorkReport>();
                customers = LoadFromFile<List<Customer>>(Path.Combine(dataDir, "customers.json")) ?? new List<Customer>();

                if (!departments.Any())
                {
                    InitializeSampleData();
                }

                // ✅ ДОБАВИТЬ СЮДА: Гарантированная очистка при каждой загрузке
                RemoveDuplicateCustomers();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                InitializeSampleData();
                RemoveDuplicateCustomers(); // И здесь тоже на всякий случай
            }
        }

        private void SaveToFile<T>(T data, string filePath)
        {
            var json = JsonSerializer.Serialize(data, jsonOptions);
            File.WriteAllText(filePath, json);
        }

        private T LoadFromFile<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath))
                return null;
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, jsonOptions);
        }

        private void InitializeSampleData()
        {
            if (!departments.Any())
            {
                AddDepartment(new ConstructionDepartment
                {
                    Name = "ООО \"Стандарт\"",
                    Address = "г. Русский, ул. Полевая",
                    DepartmentHeadId = 0  // Руководитель не назначен
                });
            }

            if (!customers.Any(c => c.Name == "Городская администрация"))
            {
                AddCustomer(new Customer
                {
                    Name = "Городская администрация"
                });
            }

            if (!customers.Any(c => c.Name == "ГК 'Дом-Строй'"))
            {
                AddCustomer(new Customer
                {
                    Name = "ГК 'Дом-Строй'"
                });
            }

            if (!customers.Any(c => c.Name == "ПАО 'Транс-Инженеринг'"))
            {
                AddCustomer(new Customer
                {
                    Name = "ПАО 'Транс-Инженеринг'"
                });
            }

            SaveData();
        }

        // ✅ МЕТОД ДЛЯ ОЧИСТКИ СУЩЕСТВУЮЩИХ ДУБЛИКАТОВ (запустить один раз)
        public void RemoveDuplicateCustomers()
        {
            var uniqueCustomers = customers
                .GroupBy(c => c.Name)  // Группируем по имени
                .Select(g => g.First())  // Берём первого из каждой группы
                .ToList();

            customers = uniqueCustomers;
            SaveData();  // Сохраняем очищенный список
        }

        // Department methods
        public void AddDepartment(ConstructionDepartment department)
        {
            department.Id = departments.Count > 0 ? departments.Max(d => d.Id) + 1 : 1;
            departments.Add(department);
            SaveData();
        }

        public List<ConstructionDepartment> GetDepartments() => departments;
        public ConstructionDepartment GetDepartment(int id) => departments.FirstOrDefault(d => d.Id == id);

        public void UpdateDepartment(ConstructionDepartment department)
        {
            var existing = GetDepartment(department.Id);
            if (existing != null)
            {
                existing.Name = department.Name;
                existing.Address = department.Address;
                existing.DepartmentHeadId = department.DepartmentHeadId;
                SaveData();
            }
        }

        public void DeleteDepartment(int departmentId)
        {
            var department = departments.FirstOrDefault(d => d.Id == departmentId);
            if (department != null)
            {
                var deptSites = sites.Where(s => s.DepartmentId == departmentId).ToList();
                foreach (var site in deptSites)
                {
                    DeleteSite(site.Id);
                }
                departments.RemoveAll(d => d.Id == departmentId);
                SaveData();
            }
        }

        // Site methods
        public void AddSite(Site site)
        {
            site.Id = sites.Count > 0 ? sites.Max(s => s.Id) + 1 : 1;
            sites.Add(site);
            SaveData();
        }

        public List<Site> GetSites() => sites;
        public List<Site> GetSitesByDepartment(int departmentId) => sites.Where(s => s.DepartmentId == departmentId).ToList();
        public Site GetSite(int id) => sites.FirstOrDefault(s => s.Id == id);

        public void UpdateSite(Site site)
        {
            var existing = GetSite(site.Id);
            if (existing != null)
            {
                existing.Name = site.Name;
                existing.DepartmentId = site.DepartmentId;
                existing.SiteManagerId = site.SiteManagerId;
                SaveData();
            }
        }

        public void DeleteSite(int siteId)
        {
            var site = sites.FirstOrDefault(s => s.Id == siteId);
            if (site != null)
            {
                var siteObjects = objects.Where(o => o.SiteId == siteId).ToList();
                foreach (var obj in siteObjects)
                {
                    DeleteObject(obj.Id);
                }
                sites.RemoveAll(s => s.Id == siteId);
                SaveData();
            }
        }

        // Object methods
        public void AddObject(ConstructionObject obj)
        {
            obj.Id = objects.Count > 0 ? objects.Max(o => o.Id) + 1 : 1;
            objects.Add(obj);
            SaveData();
        }

        public List<ConstructionObject> GetObjects() => objects;
        public List<ConstructionObject> GetObjectsBySite(int siteId) => objects.Where(o => o.SiteId == siteId).ToList();
        public List<ConstructionObject> GetObjectsByDepartment(int departmentId)
        {
            var deptSites = sites.Where(s => s.DepartmentId == departmentId).Select(s => s.Id);
            return objects.Where(o => deptSites.Contains(o.SiteId)).ToList();
        }
        public ConstructionObject GetObject(int id) => objects.FirstOrDefault(o => o.Id == id);

        public void UpdateObject(ConstructionObject obj)
        {
            var existing = GetObject(obj.Id);
            if (existing != null)
            {
                existing.Name = obj.Name;
                existing.Type = obj.Type;
                existing.SiteId = obj.SiteId;
                existing.CustomerId = obj.CustomerId;
                existing.StartDate = obj.StartDate;
                existing.PlannedEndDate = obj.PlannedEndDate;
                existing.ActualEndDate = obj.ActualEndDate;
                existing.Budget = obj.Budget;
                existing.ActualCost = obj.ActualCost;
                existing.Floors = obj.Floors;
                existing.BuildingMaterial = obj.BuildingMaterial;
                existing.ApartmentsCount = obj.ApartmentsCount;
                existing.SpanType = obj.SpanType;
                existing.Width = obj.Width;
                existing.LanesCount = obj.LanesCount;
                SaveData();
            }
        }

        public void DeleteObject(int objectId)
        {
            workSchedules.RemoveAll(ws => ws.ObjectId == objectId);
            estimates.RemoveAll(e => e.ObjectId == objectId);
            var objSchedules = workSchedules.Where(ws => ws.ObjectId == objectId).Select(ws => ws.Id);
            workReports.RemoveAll(wr => objSchedules.Contains(wr.WorkScheduleId));
            objects.RemoveAll(o => o.Id == objectId);
            SaveData();
        }

        // Employee methods
        public void AddEmployee(Employee employee)
        {
            employee.Id = employees.Count > 0 ? employees.Max(e => e.Id) + 1 : 1;
            employees.Add(employee);
            SaveData();
        }

        public List<Employee> GetEmployees() => employees;
        public List<Employee> GetEmployeesBySite(int siteId) => employees.Where(e => e.SiteId == siteId).ToList();
        public List<Employee> GetEmployeesByDepartment(int departmentId)
        {
            var deptSites = sites.Where(s => s.DepartmentId == departmentId).Select(s => s.Id);
            return employees.Where(e => e.SiteId.HasValue && deptSites.Contains(e.SiteId.Value) || !e.SiteId.HasValue).ToList();
        }
        public Employee GetEmployee(int id) => employees.FirstOrDefault(e => e.Id == id);

        public void UpdateEmployee(Employee employee)
        {
            var existing = GetEmployee(employee.Id);
            if (existing != null)
            {
                existing.FullName = employee.FullName;
                existing.Phone = employee.Phone;
                existing.Email = employee.Email;
                existing.Position = employee.Position;
                existing.SiteId = employee.SiteId;
                existing.IsBrigadeLeader = employee.IsBrigadeLeader;
                existing.Education = employee.Education;
                existing.ExperienceYears = employee.ExperienceYears;
                existing.Certification = employee.Certification;
                SaveData();
            }
        }

        public void DeleteEmployee(int employeeId)
        {
            employees.RemoveAll(e => e.Id == employeeId);
            SaveData();
        }

        // Worker methods
        public void AddWorker(Worker worker)
        {
            worker.Id = workers.Count > 0 ? workers.Max(w => w.Id) + 1 : 1;
            workers.Add(worker);
            SaveData();
        }

        public List<Worker> GetWorkers() => workers;
        public List<Worker> GetWorkersByBrigade(int brigadeId) => workers.Where(w => w.BrigadeId == brigadeId).ToList();
        public Worker GetWorker(int id) => workers.FirstOrDefault(w => w.Id == id);

        public void UpdateWorker(Worker worker)
        {
            var existing = GetWorker(worker.Id);
            if (existing != null)
            {
                existing.FullName = worker.FullName;
                existing.Phone = worker.Phone;
                existing.Email = worker.Email;
                existing.Specialty = worker.Specialty;
                existing.BrigadeId = worker.BrigadeId;
                existing.SiteId = worker.SiteId;
                existing.QualificationLevel = worker.QualificationLevel;
                existing.HireDate = worker.HireDate;
                existing.HasSafetyCertification = worker.HasSafetyCertification;
                SaveData();
            }
        }

        public void DeleteWorker(int workerId)
        {
            workers.RemoveAll(w => w.Id == workerId);
            SaveData();
        }

        // Brigade methods
        public void AddBrigade(Brigade brigade)
        {
            brigade.Id = brigades.Count > 0 ? brigades.Max(b => b.Id) + 1 : 1;
            brigades.Add(brigade);
            SaveData();
        }

        public List<Brigade> GetBrigades() => brigades;
        public List<Brigade> GetBrigadesByObject(int objectId) => brigades.Where(b => b.CurrentObjectId == objectId).ToList();
        public Brigade GetBrigade(int id) => brigades.FirstOrDefault(b => b.Id == id);

        public void UpdateBrigade(Brigade brigade)
        {
            var existing = GetBrigade(brigade.Id);
            if (existing != null)
            {
                existing.Name = brigade.Name;
                existing.LeaderId = brigade.LeaderId;
                existing.CurrentObjectId = brigade.CurrentObjectId;
                existing.CurrentWorkType = brigade.CurrentWorkType;
                SaveData();
            }
        }

        public void DeleteBrigade(int brigadeId)
        {
            foreach (var worker in workers.Where(w => w.BrigadeId == brigadeId))
            {
                worker.BrigadeId = 0;
            }
            brigades.RemoveAll(b => b.Id == brigadeId);
            SaveData();
        }

        // Equipment methods
        public void AddEquipment(Equipment equipmentItem)
        {
            equipmentItem.Id = equipment.Count > 0 ? equipment.Max(e => e.Id) + 1 : 1;
            equipment.Add(equipmentItem);
            SaveData();
        }

        public List<Equipment> GetEquipment() => equipment;
        public List<Equipment> GetEquipmentByDepartment(int departmentId)
        {
            return equipment.Where(e => e.DepartmentId == departmentId).ToList();
        }
        public List<Equipment> GetEquipmentByObject(int objectId)
        {
            return equipment.Where(e => e.CurrentObjectId == objectId).ToList();
        }
        public Equipment GetEquipment(int id) => equipment.FirstOrDefault(e => e.Id == id);

        public void UpdateEquipment(Equipment equipmentItem)
        {
            var existing = GetEquipment(equipmentItem.Id);
            if (existing != null)
            {
                existing.Name = equipmentItem.Name;
                existing.Type = equipmentItem.Type;
                existing.InventoryNumber = equipmentItem.InventoryNumber;
                existing.DepartmentId = equipmentItem.DepartmentId;
                existing.CurrentObjectId = equipmentItem.CurrentObjectId;
                existing.AssignedDate = equipmentItem.AssignedDate;
                existing.Status = equipmentItem.Status;
                SaveData();
            }
        }

        public void DeleteEquipment(int equipmentId)
        {
            equipment.RemoveAll(e => e.Id == equipmentId);
            SaveData();
        }

        // WorkSchedule methods
        public void AddWorkSchedule(WorkSchedule schedule)
        {
            schedule.Id = workSchedules.Count > 0 ? workSchedules.Max(ws => ws.Id) + 1 : 1;
            workSchedules.Add(schedule);
            SaveData();
        }

        public List<WorkSchedule> GetWorkSchedules() => workSchedules;
        public List<WorkSchedule> GetWorkSchedulesByObject(int objectId) => workSchedules.Where(ws => ws.ObjectId == objectId).ToList();
        public List<WorkSchedule> GetWorkSchedulesByWorkType(WorkType workType) => workSchedules.Where(ws => ws.WorkType == workType).ToList();
        public List<WorkSchedule> GetDelayedSchedules() => workSchedules.Where(ws => ws.Status == WorkStatus.Delayed).ToList();
        public WorkSchedule GetWorkSchedule(int id) => workSchedules.FirstOrDefault(ws => ws.Id == id);

        public void UpdateWorkSchedule(WorkSchedule schedule)
        {
            var existing = GetWorkSchedule(schedule.Id);
            if (existing != null)
            {
                existing.ObjectId = schedule.ObjectId;
                existing.WorkType = schedule.WorkType;
                existing.BrigadeId = schedule.BrigadeId;
                existing.PlannedStartDate = schedule.PlannedStartDate;
                existing.PlannedEndDate = schedule.PlannedEndDate;
                existing.ActualStartDate = schedule.ActualStartDate;
                existing.ActualEndDate = schedule.ActualEndDate;
                existing.Status = schedule.Status;
                existing.Notes = schedule.Notes;
                SaveData();
            }
        }

        public void DeleteWorkSchedule(int scheduleId)
        {
            workSchedules.RemoveAll(ws => ws.Id == scheduleId);
            SaveData();
        }

        // Estimate methods
        public void AddEstimate(Estimate estimate)
        {
            estimate.Id = estimates.Count > 0 ? estimates.Max(e => e.Id) + 1 : 1;
            estimates.Add(estimate);
            SaveData();
        }

        public List<Estimate> GetEstimates() => estimates;
        public List<Estimate> GetEstimatesByObject(int objectId) => estimates.Where(e => e.ObjectId == objectId).ToList();
        public List<Estimate> GetEstimatesWithOverrun() => estimates.Where(e => e.ActualQuantity > e.PlannedQuantity).ToList();
        public Estimate GetEstimate(int id) => estimates.FirstOrDefault(e => e.Id == id);

        public void UpdateEstimate(Estimate estimate)
        {
            var existing = GetEstimate(estimate.Id);
            if (existing != null)
            {
                existing.ObjectId = estimate.ObjectId;
                existing.MaterialName = estimate.MaterialName;
                existing.Unit = estimate.Unit;
                existing.PlannedQuantity = estimate.PlannedQuantity;
                existing.ActualQuantity = estimate.ActualQuantity;
                existing.UnitPrice = estimate.UnitPrice;
                SaveData();
            }
        }

        public void DeleteEstimate(int estimateId)
        {
            estimates.RemoveAll(e => e.Id == estimateId);
            SaveData();
        }

        // WorkReport methods
        public void AddWorkReport(WorkReport report)
        {
            report.Id = workReports.Count > 0 ? workReports.Max(wr => wr.Id) + 1 : 1;
            workReports.Add(report);
            SaveData();
        }

        public List<WorkReport> GetWorkReports() => workReports;
        public List<WorkReport> GetWorkReportsByObject(int objectId)
        {
            var objSchedules = workSchedules.Where(ws => ws.ObjectId == objectId).Select(ws => ws.Id);
            return workReports.Where(wr => objSchedules.Contains(wr.WorkScheduleId)).ToList();
        }
        public List<WorkReport> GetWorkReportsByPeriod(DateTime from, DateTime to) => workReports.Where(wr => wr.ReportDate >= from && wr.ReportDate <= to).ToList();
        public WorkReport GetWorkReport(int id) => workReports.FirstOrDefault(wr => wr.Id == id);

        public void UpdateWorkReport(WorkReport report)
        {
            var existing = GetWorkReport(report.Id);
            if (existing != null)
            {
                existing.WorkScheduleId = report.WorkScheduleId;
                existing.ReportDate = report.ReportDate;
                existing.CompletedPercentage = report.CompletedPercentage;
                existing.Description = report.Description;
                existing.AdditionalCosts = report.AdditionalCosts;
                existing.Issues = report.Issues;
                existing.ReportedBy = report.ReportedBy;
                SaveData();
            }
        }

        public void DeleteWorkReport(int reportId)
        {
            workReports.RemoveAll(wr => wr.Id == reportId);
            SaveData();
        }

        // Customer methods
        public void AddCustomer(Customer customer)
        {
            customer.Id = customers.Count > 0 ? customers.Max(c => c.Id) + 1 : 1;
            customers.Add(customer);
            SaveData();
        }

        public List<Customer> GetCustomers() => customers;
        public Customer GetCustomer(int id) => customers.FirstOrDefault(c => c.Id == id);

        public void UpdateCustomer(Customer customer)
        {
            var existing = GetCustomer(customer.Id);
            if (existing != null)
            {
                existing.Name = customer.Name;
                SaveData();
            }
        }

        public void DeleteCustomer(int customerId)
        {
            customers.RemoveAll(c => c.Id == customerId);
            SaveData();
        }

        // Helper methods
        public List<ConstructionObject> GetObjectsByWorkTypeAndPeriod(WorkType workType, DateTime from, DateTime to)
        {
            var scheduleIds = workSchedules
                .Where(ws => ws.WorkType == workType &&
                           ws.PlannedStartDate >= from &&
                           ws.PlannedEndDate <= to)
                .Select(ws => ws.ObjectId);
            return objects.Where(o => scheduleIds.Contains(o.Id)).ToList();
        }

        public decimal GetTotalBudgetByDepartment(int departmentId)
        {
            var deptObjects = GetObjectsByDepartment(departmentId);
            return deptObjects.Sum(o => o.Budget);
        }

        public decimal GetTotalActualCostByDepartment(int departmentId)
        {
            var deptObjects = GetObjectsByDepartment(departmentId);
            return deptObjects.Sum(o => o.ActualCost);
        }
    }
}