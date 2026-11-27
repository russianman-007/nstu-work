using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public class PersonnelManagementForm : Form
    {
        private readonly IConstructionRepository repository;
        private DataGridView personnelGrid;
        private Button btnAddEmployee;
        private ContextMenuStrip contextMenuStrip;
        private ComboBox cmbFilterPosition;
        private ComboBox cmbFilterType;

        public PersonnelManagementForm(IConstructionRepository repo)
        {
            repository = repo;
            InitializeForm();
            LoadPersonnel();
        }

        private void InitializeForm()
        {
            this.Text = "Управление персоналом";
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            personnelGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };

            personnelGrid.DataBindingComplete += (s, e) => AddRowNumbers(personnelGrid);

            SetupPersonnelGrid();

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Редактировать", null, ContextMenu_Edit_Click);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add("Удалить", null, ContextMenu_Delete_Click);

            personnelGrid.ContextMenuStrip = contextMenuStrip;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke
            };

            btnAddEmployee = new Button
            {
                Text = "Добавить сотрудника",
                Location = new Point(10, 10),
                Size = new Size(160, 30)
            };
            btnAddEmployee.Click += BtnAddEmployee_Click;

            var separator = new Label
            {
                Location = new Point(181, 8),
                Size = new Size(2, 34),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightGray
            };

            cmbFilterPosition = new ComboBox
            {
                Location = new Point(194, 12),
                Width = 182,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterPosition.Items.Add("Все должности");
            cmbFilterPosition.Items.AddRange(GetAllPositionsInRussian());
            cmbFilterPosition.SelectedIndex = 0;
            cmbFilterPosition.SelectedIndexChanged += (s, e) => LoadPersonnel();

            cmbFilterType = new ComboBox
            {
                Location = new Point(390, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterType.Items.Add("Все типы");
            cmbFilterType.Items.Add("Инженерно-техннический персонал");
            cmbFilterType.Items.Add("Рабочий");
            cmbFilterType.SelectedIndex = 0;
            cmbFilterType.SelectedIndexChanged += (s, e) => LoadPersonnel();

            topPanel.Controls.Add(btnAddEmployee);
            topPanel.Controls.Add(separator);
            topPanel.Controls.Add(cmbFilterPosition);
            topPanel.Controls.Add(cmbFilterType);

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(personnelGrid);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);
        }

        private void SetupPersonnelGrid()
        {
            personnelGrid.Columns.Clear();

            personnelGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            personnelGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Type",
                HeaderText = "Тип",
                DataPropertyName = "Type",
                Visible = false
            });

            personnelGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            personnelGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                HeaderText = "ФИО",
                DataPropertyName = "FullName",
                FillWeight = 1
            });
            personnelGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Position",
                HeaderText = "Должность",
                DataPropertyName = "Position",
                FillWeight = 1
            });
            personnelGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EmployeeType",
                HeaderText = "Тип",
                DataPropertyName = "EmployeeType",
                FillWeight = 1
            });
            personnelGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Phone",
                HeaderText = "Телефон",
                DataPropertyName = "Phone",
                FillWeight = 1
            });
            personnelGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Email",
                HeaderText = "Email",
                DataPropertyName = "Email",
                FillWeight = 1
            });
            personnelGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Experience",
                HeaderText = "Стаж (лет)",
                DataPropertyName = "Experience",
                FillWeight = 1
            });
        }

        private void LoadPersonnel()
        {
            var employees = repository.GetEmployees().Select(e => new
            {
                e.Id,
                e.FullName,
                Position = GetPositionInRussian(e.Position),
                EmployeeType = "Инженерно-технический персонал",
                Type = "Employee",
                Phone = e.Phone,
                Email = string.IsNullOrWhiteSpace(e.Email) ? "-" : e.Email,
                Experience = e.ExperienceYears?.ToString() ?? "-"
            }).ToList();

            var workers = repository.GetWorkers().Select(w => new
            {
                w.Id,
                w.FullName,
                Position = GetWorkerSpecialtyInRussian(w.Specialty),
                EmployeeType = "Рабочий",
                Type = "Worker",
                Phone = w.Phone,
                Email = string.IsNullOrWhiteSpace(w.Email) ? "-" : w.Email,
                Experience = w.HireDate.HasValue ? $"{(DateTime.Now.Year - w.HireDate.Value.Year)}" : "-"
            }).ToList();

            var personnel = employees.Concat(workers).ToList();

            if (cmbFilterPosition.SelectedIndex > 0)
            {
                var selectedPosition = cmbFilterPosition.SelectedItem.ToString();
                personnel = personnel.Where(p => p.Position == selectedPosition).ToList();
            }

            if (cmbFilterType.SelectedIndex > 0)
            {
                var selectedType = cmbFilterType.SelectedItem.ToString();
                personnel = personnel.Where(p => p.EmployeeType == selectedType).ToList();
            }

            personnelGrid.DataSource = personnel.Select(p => new
            {
                p.Id,
                p.FullName,
                p.Position,
                p.EmployeeType,
                p.Type,
                p.Phone,
                p.Email,
                p.Experience
            }).ToList();
        }

        private string[] GetAllPositionsInRussian()
        {
            var positions = Enum.GetValues(typeof(Position)).Cast<Position>();
            var workerSpecialties = Enum.GetValues(typeof(WorkerSpecialty)).Cast<WorkerSpecialty>();
            return positions.Select(p => GetPositionInRussian(p))
                .Concat(workerSpecialties.Select(w => GetWorkerSpecialtyInRussian(w)))
                .ToArray();
        }

        private string GetPositionInRussian(Position position)
        {
            switch (position)
            {
                case Position.Engineer: return "Инженер";
                case Position.Technologist: return "Технолог";
                case Position.Technician: return "Техник";
                case Position.Foreman: return "Прораб";
                case Position.Master: return "Мастер";
                case Position.SiteManager: return "Начальник участка";
                case Position.DepartmentHead: return "Руководитель";
                default: return position.ToString();
            }
        }

        private string GetWorkerSpecialtyInRussian(WorkerSpecialty specialty)
        {
            switch (specialty)
            {
                case WorkerSpecialty.Mason: return "Каменщик";
                case WorkerSpecialty.ConcreteWorker: return "Бетонщик";
                case WorkerSpecialty.Finisher: return "Отделочник";
                case WorkerSpecialty.Welder: return "Сварщик";
                case WorkerSpecialty.Electrician: return "Электрик";
                case WorkerSpecialty.Driver: return "Водитель";
                case WorkerSpecialty.Mechanic: return "Механик";
                default: return specialty.ToString();
            }
        }

        private Position? GetPositionFromRussian(string russianName)
        {
            switch (russianName)
            {
                case "Инженер": return Position.Engineer;
                case "Технолог": return Position.Technologist;
                case "Техник": return Position.Technician;
                case "Прораб": return Position.Foreman;
                case "Мастер": return Position.Master;
                case "Начальник участка": return Position.SiteManager;
                case "Руководитель": return Position.DepartmentHead;
                default: return null;
            }
        }

        private WorkerSpecialty? GetWorkerSpecialtyFromRussian(string russianName)
        {
            switch (russianName)
            {
                case "Каменщик": return WorkerSpecialty.Mason;
                case "Бетонщик": return WorkerSpecialty.ConcreteWorker;
                case "Отделочник": return WorkerSpecialty.Finisher;
                case "Сварщик": return WorkerSpecialty.Welder;
                case "Электрик": return WorkerSpecialty.Electrician;
                case "Водитель": return WorkerSpecialty.Driver;
                case "Механик": return WorkerSpecialty.Mechanic;
                default: return null;
            }
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

        private int GetSelectedEmployeeId()
        {
            if (personnelGrid.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Выберите сотрудника.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return -1;
            }
            return (int)personnelGrid.CurrentRow.Cells["Id"].Value;
        }

        private void BtnAddEmployee_Click(object sender, EventArgs e)
        {
            var form = new AddEmployeeForm(repository);
            if (form.ShowDialog() == DialogResult.OK) LoadPersonnel();
        }

        private void ContextMenu_Edit_Click(object? sender, EventArgs e)
        {
            if (personnelGrid.CurrentRow?.DataBoundItem == null) { MessageBox.Show("Выберите сотрудника."); return; }
            int id = (int)personnelGrid.CurrentRow.Cells["Id"].Value;
            string type = personnelGrid.CurrentRow.Cells["Type"].Value?.ToString();

            if (type == "Worker")
            {
                var worker = repository.GetWorker(id);
                if (worker == null) { MessageBox.Show("Рабочий не найден."); return; }
                var form = new AddEmployeeForm(repository, worker);
                if (form.ShowDialog() == DialogResult.OK) LoadPersonnel();
            }
            else
            {
                var employee = repository.GetEmployee(id);
                if (employee == null) { MessageBox.Show("Сотрудник не найден."); return; }
                var form = new AddEmployeeForm(repository, employee);
                if (form.ShowDialog() == DialogResult.OK) LoadPersonnel();
            }
        }

        private void ContextMenu_Delete_Click(object? sender, EventArgs e)
        {
            if (personnelGrid.CurrentRow?.DataBoundItem == null) { MessageBox.Show("Выберите сотрудника."); return; }
            int id = (int)personnelGrid.CurrentRow.Cells["Id"].Value;
            string type = personnelGrid.CurrentRow.Cells["Type"].Value?.ToString();

            if (type == "Worker")
            {
                var worker = repository.GetWorker(id);
                if (worker != null && MessageBox.Show($"Удалить рабочего \"{worker.FullName}\"?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                { repository.DeleteWorker(id); LoadPersonnel(); }
            }
            else
            {
                var employee = repository.GetEmployee(id);
                if (employee != null && MessageBox.Show($"Удалить сотрудника \"{employee.FullName}\"?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                { repository.DeleteEmployee(id); LoadPersonnel(); }
            }
        }
    }

    // Форма добавления/редактирования сотрудника/рабочего
    public class AddEmployeeForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly Employee editingEmployee;
        private readonly Worker editingWorker;
        private TextBox txtFullName;
        private MaskedTextBox txtPhone;  // ✅ ИЗМЕНЕНО: MaskedTextBox вместо TextBox
        private TextBox txtEmail;
        private ComboBox cmbPosition;
        private TextBox txtExperience;

        private const int LABEL_WIDTH = 100;
        private const int FIELD_X = 120;
        private const int FIELD_WIDTH = 280;
        private const int ROW_HEIGHT = 35;
        private const int START_Y = 20;

        public AddEmployeeForm(IConstructionRepository repo) : this(repo, null, null) { }
        public AddEmployeeForm(IConstructionRepository repo, Employee employeeToEdit) : this(repo, employeeToEdit, null) { }
        public AddEmployeeForm(IConstructionRepository repo, Worker workerToEdit) : this(repo, null, workerToEdit) { }

        private AddEmployeeForm(IConstructionRepository repo, Employee employeeToEdit, Worker workerToEdit)
        {
            repository = repo;
            editingEmployee = employeeToEdit;
            editingWorker = workerToEdit;
            InitializeForm();
            if (editingEmployee != null || editingWorker != null) FillFieldsForEdit();
        }

        private void InitializeForm()
        {
            this.Text = editingEmployee != null ? "Редактировать сотрудника" :
                        editingWorker != null ? "Редактировать рабочего" : "Добавить сотрудника";
            this.Size = new Size(450, 295);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = START_Y;

            Controls.Add(new Label { Text = "* ФИО:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            Controls.Add(txtFullName = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH });
            yPos += ROW_HEIGHT;

            Controls.Add(new Label { Text = "* Должность:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            Controls.Add(cmbPosition = new ComboBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH, DropDownStyle = ComboBoxStyle.DropDownList });
            yPos += ROW_HEIGHT;

            // ✅ Заполняем ComboBox только теми значениями, которые относятся к типу редактируемой записи.
            // Если редактируется рабочий — только специальности рабочих.
            // Если редактируется ИТР — только должности ИТР.
            // Если создаётся новая запись — оба списка (пользователь сам выберет тип).
            if (editingWorker != null)
            {
                var specialties = Enum.GetValues(typeof(WorkerSpecialty)).Cast<WorkerSpecialty>();
                foreach (var spec in specialties) cmbPosition.Items.Add(GetWorkerSpecialtyInRussian(spec));
            }
            else if (editingEmployee != null)
            {
                var positions = Enum.GetValues(typeof(Position)).Cast<Position>();
                foreach (var pos in positions) cmbPosition.Items.Add(GetPositionInRussian(pos));
            }
            else
            {
                var positions = Enum.GetValues(typeof(Position)).Cast<Position>();
                var specialties = Enum.GetValues(typeof(WorkerSpecialty)).Cast<WorkerSpecialty>();
                foreach (var pos in positions) cmbPosition.Items.Add(GetPositionInRussian(pos));
                foreach (var spec in specialties) cmbPosition.Items.Add(GetWorkerSpecialtyInRussian(spec));
            }

            Controls.Add(new Label { Text = "* Телефон:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            Controls.Add(txtPhone = new MaskedTextBox
            {
                Location = new Point(FIELD_X, yPos),
                Width = FIELD_WIDTH,
                Mask = "+7 (000) 000-00-00",
                PromptChar = '_'
            });
            yPos += ROW_HEIGHT;

            Controls.Add(new Label { Text = "Email:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            Controls.Add(txtEmail = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH });
            yPos += ROW_HEIGHT;

            Controls.Add(new Label { Text = "Стаж (лет):", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            Controls.Add(txtExperience = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH });
            yPos += ROW_HEIGHT + 10;

            var btnSave = new Button { Text = "Сохранить", Location = new Point(FIELD_X, yPos), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(FIELD_X + 120, yPos), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSave); Controls.Add(btnCancel);
        }

        private void FillFieldsForEdit()
        {
            if (editingEmployee != null)
            {
                txtFullName.Text = editingEmployee.FullName;
                cmbPosition.SelectedItem = GetPositionInRussian(editingEmployee.Position);
                txtPhone.Text = editingEmployee.Phone;
                txtEmail.Text = editingEmployee.Email;
                txtExperience.Text = editingEmployee.ExperienceYears?.ToString() ?? "";
            }
            else if (editingWorker != null)
            {
                txtFullName.Text = editingWorker.FullName;
                cmbPosition.SelectedItem = GetWorkerSpecialtyInRussian(editingWorker.Specialty);
                txtPhone.Text = "";
                txtEmail.Text = "";
                txtExperience.Text = editingWorker.HireDate.HasValue ? $"{(DateTime.Now.Year - editingWorker.HireDate.Value.Year)}" : "";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            bool nameEmpty = string.IsNullOrWhiteSpace(txtFullName.Text);
            bool positionEmpty = cmbPosition.SelectedIndex < 0;
            bool phoneEmpty = !txtPhone.MaskCompleted;

            // Общее сообщение выводится только когда не заполнено ни одно обязательное поле
            if (nameEmpty && positionEmpty && phoneEmpty)
            {
                MessageBox.Show("Заполните обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Если заполнено частично, указываем конкретное незаполненное поле
            if (nameEmpty)
            {
                MessageBox.Show("Введите ФИО сотрудника!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtFullName.Focus();
                return;
            }

            // Проверка ФИО: минимум два слова, в каждом минимум два символа.
            // Разделителем слов служит пробел, поэтому при двух словах требуется минимум один пробел
            string[] nameParts = txtFullName.Text.Trim()
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length < 2)
            {
                MessageBox.Show("ФИО должно содержать минимум 2 слова, разделённых пробелом (например: Иванов Иван)!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            foreach (string part in nameParts)
            {
                if (part.Length < 2)
                {
                    MessageBox.Show("Каждое слово в ФИО должно содержать минимум 2 символа!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFullName.Focus();
                    return;
                }
            }

            if (positionEmpty)
            {
                MessageBox.Show("Выберите должность!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbPosition.Focus();
                return;
            }

            // Проверка: телефон обязателен
            if (string.IsNullOrWhiteSpace(txtPhone.Text) || txtPhone.MaskCompleted == false)
            {
                MessageBox.Show("Введите корректный номер телефона в формате: +7 (XXX) XXX-XX-XX", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPhone.Focus();
                return;
            }

            // Проверка: Email (если заполнен)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!txtEmail.Text.Contains("@"))
                {
                    MessageBox.Show("Email должен содержать символ '@'!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtEmail.Focus();
                    return;
                }
                int atIndex = txtEmail.Text.IndexOf("@");
                string domainPart = txtEmail.Text.Substring(atIndex + 1);
                if (domainPart.Length < 4)
                {
                    MessageBox.Show("После символа '@' должно быть не менее 4 символов!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtEmail.Focus();
                    return;
                }
            }

            int? siteId = null;
            var position = GetPositionFromRussian(cmbPosition.SelectedItem.ToString());
            var specialty = GetWorkerSpecialtyFromRussian(cmbPosition.SelectedItem.ToString());
            if (specialty.HasValue)
            {
                if (editingWorker == null)
                {
                    repository.AddWorker(new Worker
                    {
                        FullName = txtFullName.Text.Trim(),
                        Specialty = specialty.Value,
                        SiteId = siteId,
                        HireDate = int.TryParse(txtExperience.Text, out int exp) ? DateTime.Now.AddYears(-exp) : (DateTime?)null
                    });
                }
                else
                {
                    editingWorker.FullName = txtFullName.Text.Trim();
                    editingWorker.Specialty = specialty.Value;
                    editingWorker.SiteId = siteId;
                    editingWorker.HireDate = int.TryParse(txtExperience.Text, out int exp) ? DateTime.Now.AddYears(-exp) : (DateTime?)null;
                    repository.UpdateWorker(editingWorker);
                }
            }
            else if (position.HasValue)
            {
                if (editingEmployee == null)
                {
                    repository.AddEmployee(new Employee
                    {
                        FullName = txtFullName.Text.Trim(),
                        Position = position.Value,
                        SiteId = siteId,
                        Phone = txtPhone.Text.Trim(),
                        Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                        ExperienceYears = int.TryParse(txtExperience.Text, out int exp) ? exp : (int?)null
                    });
                }
                else
                {
                    editingEmployee.FullName = txtFullName.Text.Trim();
                    editingEmployee.Position = position.Value;
                    editingEmployee.SiteId = siteId;
                    editingEmployee.Phone = txtPhone.Text.Trim();
                    editingEmployee.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
                    editingEmployee.ExperienceYears = int.TryParse(txtExperience.Text, out int exp) ? exp : (int?)null;
                    repository.UpdateEmployee(editingEmployee);
                }
            }
            else
            {
                MessageBox.Show("Неизвестная должность!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("Запись успешно сохранена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK; Close();
        }

        private string GetPositionInRussian(Position position)
        {
            switch (position)
            {
                case Position.Engineer: return "Инженер";
                case Position.Technologist: return "Технолог";
                case Position.Technician: return "Техник";
                case Position.Foreman: return "Прораб";
                case Position.Master: return "Мастер";
                case Position.SiteManager: return "Начальник участка";
                case Position.DepartmentHead: return "Руководитель";
                default: return position.ToString();
            }
        }

        private string GetWorkerSpecialtyInRussian(WorkerSpecialty specialty)
        {
            switch (specialty)
            {
                case WorkerSpecialty.Mason: return "Каменщик";
                case WorkerSpecialty.ConcreteWorker: return "Бетонщик";
                case WorkerSpecialty.Finisher: return "Отделочник";
                case WorkerSpecialty.Welder: return "Сварщик";
                case WorkerSpecialty.Electrician: return "Электрик";
                case WorkerSpecialty.Driver: return "Водитель";
                case WorkerSpecialty.Mechanic: return "Механик";
                default: return specialty.ToString();
            }
        }

        private Position? GetPositionFromRussian(string russianName)
        {
            switch (russianName)
            {
                case "Инженер": return Position.Engineer;
                case "Технолог": return Position.Technologist;
                case "Техник": return Position.Technician;
                case "Прораб": return Position.Foreman;
                case "Мастер": return Position.Master;
                case "Начальник участка": return Position.SiteManager;
                case "Руководитель": return Position.DepartmentHead;
                default: return null;
            }
        }

        private WorkerSpecialty? GetWorkerSpecialtyFromRussian(string russianName)
        {
            switch (russianName)
            {
                case "Каменщик": return WorkerSpecialty.Mason;
                case "Бетонщик": return WorkerSpecialty.ConcreteWorker;
                case "Отделочник": return WorkerSpecialty.Finisher;
                case "Сварщик": return WorkerSpecialty.Welder;
                case "Электрик": return WorkerSpecialty.Electrician;
                case "Водитель": return WorkerSpecialty.Driver;
                case "Механик": return WorkerSpecialty.Mechanic;
                default: return null;
            }
        }
    }
}