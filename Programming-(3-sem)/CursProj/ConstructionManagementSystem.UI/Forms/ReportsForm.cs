using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Globalization;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public class ReportsForm : Form
    {
        private readonly IConstructionRepository repository;
        private DataGridView reportGrid;
        private Button btnGenerate;
        private ComboBox cmbReportType;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;

        public ReportsForm(IConstructionRepository repo)
        {
            repository = repo;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Отчеты и аналитика";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke
            };

            btnGenerate = new Button
            {
                Text = "Сформировать",
                Location = new Point(10, 10),
                Size = new Size(160, 30)
            };
            btnGenerate.Click += BtnGenerate_Click;

            var separator = new Label
            {
                Location = new Point(181, 8),
                Size = new Size(2, 34),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightGray
            };

            cmbReportType = new ComboBox
            {
                Location = new Point(194, 12),
                Width = 205,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbReportType.Items.AddRange(new object[]
            {
                "Загруженность участков",
                "Задержки по графикам работ",
                "Перерасход материалов",
                "Использование техники",
                "Финансовая отчетность",
                "Работы по периоду"
            });
            cmbReportType.SelectedIndex = 0;

            var lblPeriod = new Label { Text = "Период:", Location = new Point(410, 14), AutoSize = true };

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(475, 12),
                Width = 110,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(-1)
            };

            var lblTo = new Label { Text = "по", Location = new Point(590, 14), AutoSize = true };

            dtpToDate = new DateTimePicker
            {
                Location = new Point(617, 12),
                Width = 110,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };

            topPanel.Controls.AddRange(new Control[]
            {
                btnGenerate, separator, cmbReportType, lblPeriod, dtpFromDate, lblTo, dtpToDate
            });

            reportGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(reportGrid);
            mainPanel.Controls.Add(topPanel);
            this.Controls.Add(mainPanel);
        }

        private void AddRowNumbers(DataGridView grid, string columnName = "RowNumber")
        {
            if (!grid.Columns.Contains(columnName)) return;
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                if (grid.Rows[i].IsNewRow) continue;
                if (grid.Rows[i].Cells[columnName] != null)
                {
                    grid.Rows[i].Cells[columnName].Value = (i + 1).ToString();
                }
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            switch (cmbReportType.SelectedIndex)
            {
                case 0: GenerateSiteLoadReport(); break;
                case 1: GenerateDelayedWorksReport(); break;
                case 2: GenerateMaterialOverrunReport(); break;
                case 3: GenerateEquipmentUsageReport(); break;
                case 4: GenerateFinancialReport(); break;
                case 5: GenerateWorksByPeriodReport(); break;
            }
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ПЕРЕВОДА ===

        private string GetWorkTypeInRussian(WorkType type)
        {
            switch (type)
            {
                case WorkType.Foundation: return "Фундамент";
                case WorkType.Brickwork: return "Кладка";
                case WorkType.Plumbing: return "Сантехника";
                case WorkType.Electrical: return "Электрика";
                case WorkType.Finishing: return "Отделка";
                case WorkType.Roadwork: return "Дорожные работы";
                default: return type.ToString();
            }
        }

        private string GetEquipmentTypeInRussian(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Crane: return "Кран";
                case EquipmentType.Excavator: return "Экскаватор";
                case EquipmentType.Bulldozer: return "Бульдозер";
                case EquipmentType.PileDriver: return "Копёр";
                case EquipmentType.Truck: return "Грузовик";
                case EquipmentType.DumpTruck: return "Самосвал";
                case EquipmentType.ConcreteMixer: return "Бетономешалка";
                default: return type.ToString();
            }
        }

        private string GetObjectTypeInRussian(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.ResidentialBuilding: return "Жилой дом";
                case ObjectType.Hospital: return "Больница";
                case ObjectType.School: return "Школа";
                case ObjectType.Bridge: return "Мост";
                case ObjectType.Road: return "Дорога";
                default: return type.ToString();
            }
        }

        private string GetWorkStatusInRussian(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "-";
            switch (status.ToLower())
            {
                case "completed": return "Завершен";
                case "in progress": return "В работе";
                case "planned": return "Запланирован";
                case "delayed": return "Задержан";
                case "available": return "Свободна";
                case "in use": return "В работе";
                case "maintenance": return "На обслуживании";
                case "broken": return "Неисправна";
                default: return status;
            }
        }

        // === МЕТОДЫ ГЕНЕРАЦИИ ОТЧЕТОВ ===

        private void GenerateSiteLoadReport()
        {
            var sites = repository.GetSites().Select(s => new
            {
                Site = s.Name,
                Department = repository.GetDepartment(s.DepartmentId)?.Name ?? "Неизвестно",
                Manager = repository.GetEmployee(s.SiteManagerId)?.FullName ?? "Не назначен",
                ObjectsCount = repository.GetObjectsBySite(s.Id).Count,
                BrigadesCount = repository.GetBrigades().Count(b => b.CurrentObjectId != 0 &&
                    repository.GetObjectsBySite(s.Id).Any(o => o.Id == b.CurrentObjectId)),
                EquipmentCount = repository.GetEquipment().Count(eq => eq.CurrentObjectId != 0 &&
                    repository.GetObjectsBySite(s.Id).Any(o => o.Id == eq.CurrentObjectId))
            }).ToList();

            reportGrid.Columns.Clear();
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "№",
                Width = 25,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.White
                }
            });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Участок", DataPropertyName = "Site", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Управление", DataPropertyName = "Department", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Менеджер", DataPropertyName = "Manager", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Объектов", DataPropertyName = "ObjectsCount", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Бригад", DataPropertyName = "BrigadesCount", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Техники", DataPropertyName = "EquipmentCount", FillWeight = 1 });

            reportGrid.DataSource = sites;
            AddRowNumbers(reportGrid);
        }

        private void GenerateDelayedWorksReport()
        {
            var delayedSchedules = repository.GetDelayedSchedules().Select(s => new
            {
                Object = repository.GetObject(s.ObjectId)?.Name ?? "Неизвестно",
                WorkType = GetWorkTypeInRussian(s.WorkType),  // ✅ РУССКИЙ перевод
                Brigade = repository.GetBrigade(s.BrigadeId)?.Name ?? "Неизвестно",
                PlannedStart = s.PlannedStartDate.ToString("dd.MM.yyyy"),
                PlannedEnd = s.PlannedEndDate.ToString("dd.MM.yyyy"),
                ActualEnd = s.ActualEndDate?.ToString("dd.MM.yyyy") ?? "-",
                DelayDays = s.ActualEndDate != null ? (s.ActualEndDate.Value - s.PlannedEndDate).Days : (DateTime.Now - s.PlannedEndDate).Days,
                Notes = s.Notes ?? "-"
            }).ToList();

            reportGrid.Columns.Clear();
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "№",
                Width = 25,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.White
                }
            });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Объект", DataPropertyName = "Object", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Вид работ", DataPropertyName = "WorkType", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Бригада", DataPropertyName = "Brigade", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "План. начало", DataPropertyName = "PlannedStart", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "План. окончание", DataPropertyName = "PlannedEnd", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Факт. окончание", DataPropertyName = "ActualEnd", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Задержка (дн.)", DataPropertyName = "DelayDays", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Примечания", DataPropertyName = "Notes", FillWeight = 1 });

            reportGrid.DataSource = delayedSchedules;
            AddRowNumbers(reportGrid);
        }

        private void GenerateMaterialOverrunReport()
        {
            var ruCulture = new CultureInfo("ru-RU");
            var overruns = repository.GetEstimatesWithOverrun().Select(e => new
            {
                Object = repository.GetObject(e.ObjectId)?.Name ?? "Неизвестно",
                Material = e.MaterialName,
                Unit = e.Unit,
                PlannedQty = e.PlannedQuantity.ToString("F2"),
                ActualQty = e.ActualQuantity.ToString("F2"),
                OverrunQty = (e.ActualQuantity - e.PlannedQuantity).ToString("F2"),
                UnitPrice = e.UnitPrice.ToString("C", ruCulture),
                OverrunCost = (e.ActualCost - e.TotalCost).ToString("C", ruCulture)
            }).ToList();

            reportGrid.Columns.Clear();
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "№",
                Width = 25,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.White
                }
            });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Объект", DataPropertyName = "Object", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Материал", DataPropertyName = "Material", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ед. изм.", DataPropertyName = "Unit", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "План. кол-во", DataPropertyName = "PlannedQty", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Факт. кол-во", DataPropertyName = "ActualQty", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Перерасход (кол-во)", DataPropertyName = "OverrunQty", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Цена за ед.", DataPropertyName = "UnitPrice", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Перерасход (стоимость)", DataPropertyName = "OverrunCost", FillWeight = 1 });

            reportGrid.DataSource = overruns;
            AddRowNumbers(reportGrid);
        }

        private void GenerateEquipmentUsageReport()
        {
            var equipment = repository.GetEquipment().Select(e => new
            {
                Name = e.Name,
                Type = GetEquipmentTypeInRussian(e.Type),  // ✅ РУССКИЙ перевод
                InventoryNumber = e.InventoryNumber,
                Department = repository.GetDepartment(e.DepartmentId)?.Name ?? "Неизвестно",
                Object = e.CurrentObjectId != 0 ? repository.GetObject(e.CurrentObjectId)?.Name ?? "Не назначен" : "Не назначен",
                Status = GetWorkStatusInRussian(e.Status),  // ✅ РУССКИЙ перевод статуса
                AssignedDate = e.AssignedDate?.ToString("dd.MM.yyyy") ?? "-"
            }).ToList();

            reportGrid.Columns.Clear();
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "№",
                Width = 25,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.White
                }
            });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = "Name", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Тип", DataPropertyName = "Type", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Инв. номер", DataPropertyName = "InventoryNumber", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Управление", DataPropertyName = "Department", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Объект", DataPropertyName = "Object", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = "Status", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата назначения", DataPropertyName = "AssignedDate", FillWeight = 1 });

            reportGrid.DataSource = equipment;
            AddRowNumbers(reportGrid);
        }

        private void GenerateFinancialReport()
        {
            var ruCulture = new CultureInfo("ru-RU");
            var objects = repository.GetObjects().Select(o => new
            {
                Object = o.Name,
                Type = GetObjectTypeInRussian(o.Type),  // ✅ РУССКИЙ перевод
                Customer = repository.GetCustomer(o.CustomerId)?.Name ?? "Неизвестно",
                Budget = o.Budget.ToString("C", ruCulture),
                ActualCost = o.ActualCost.ToString("C", ruCulture),
                Overrun = o.ActualCost > o.Budget ? (o.ActualCost - o.Budget).ToString("C", ruCulture) : "-",
                 // ✅ РУССКИЙ перевод статуса
            }).ToList();

            reportGrid.Columns.Clear();
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "№",
                Width = 25,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.White
                }
            });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Объект", DataPropertyName = "Object", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Тип", DataPropertyName = "Type", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Заказчик", DataPropertyName = "Customer", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Бюджет", DataPropertyName = "Budget", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Факт. затраты", DataPropertyName = "ActualCost", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Перерасход", DataPropertyName = "Overrun", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = "Status", FillWeight = 1 });

            reportGrid.DataSource = objects;
            AddRowNumbers(reportGrid);
        }

        private void GenerateWorksByPeriodReport()
        {
            var fromDate = dtpFromDate.Value;
            var toDate = dtpToDate.Value;

            var schedules = repository.GetWorkSchedules()
                .Where(s => s.PlannedStartDate >= fromDate && s.PlannedEndDate <= toDate)
                .Select(s => new
                {
                    Object = repository.GetObject(s.ObjectId)?.Name ?? "Неизвестно",
                    WorkType = GetWorkTypeInRussian(s.WorkType),  // ✅ РУССКИЙ перевод
                    Brigade = repository.GetBrigade(s.BrigadeId)?.Name ?? "Неизвестно",
                    StartDate = s.PlannedStartDate.ToString("dd.MM.yyyy"),
                    EndDate = s.PlannedEndDate.ToString("dd.MM.yyyy")
                }).ToList();

            reportGrid.Columns.Clear();
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "№",
                Width = 25,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.White
                }
            });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Объект", DataPropertyName = "Object", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Вид работ", DataPropertyName = "WorkType", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Бригада", DataPropertyName = "Brigade", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Начало", DataPropertyName = "StartDate", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Окончание", DataPropertyName = "EndDate", FillWeight = 1 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = "Status", FillWeight = 1 });

            reportGrid.DataSource = schedules;
            AddRowNumbers(reportGrid);
        }
    }
}