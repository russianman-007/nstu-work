using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.ComponentModel;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public class EquipmentManagementForm : Form
    {
        private readonly IConstructionRepository repository;
        private DataGridView equipmentGrid;
        private Button btnAddEquipment;
        private ContextMenuStrip contextMenuStrip;
        private ComboBox cmbFilterDepartment;
        private ComboBox cmbFilterType;

        public EquipmentManagementForm(IConstructionRepository repo)
        {
            repository = repo;
            InitializeForm();
            LoadEquipment();
        }

        private void InitializeForm()
        {
            this.Text = "Управление техникой";
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            equipmentGrid = new DataGridView
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

            equipmentGrid.DataBindingComplete += (s, e) => AddRowNumbers(equipmentGrid);

            SetupEquipmentGrid();

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Редактировать", null, ContextMenu_Edit_Click);
            contextMenuStrip.Items.Add("Назначить на объект", null, ContextMenu_Assign_Click);

            // ✅ НОВЫЙ пункт: Освободить технику
            var freeMenuItem = new ToolStripMenuItem("Освободить технику", null, ContextMenu_FreeEquipment_Click);
            freeMenuItem.Name = "FreeMenuItem";
            contextMenuStrip.Items.Add(freeMenuItem);

            contextMenuStrip.Items.Add(new ToolStripSeparator());

            // ✅ Пункт неисправности
            var brokenMenuItem = new ToolStripMenuItem("Объявить о неисправности", null, ContextMenu_ToggleBroken_Click);
            brokenMenuItem.Name = "BrokenMenuItem";
            contextMenuStrip.Items.Add(brokenMenuItem);

            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add("Удалить", null, ContextMenu_Delete_Click);

            // ✅ ОБНОВЛЯЕМ пункты при открытии меню
            contextMenuStrip.Opening += ContextMenuStrip_Opening;

            equipmentGrid.ContextMenuStrip = contextMenuStrip;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke
            };

            btnAddEquipment = new Button
            {
                Text = "Добавить технику",
                Location = new Point(10, 10),
                Size = new Size(160, 30)
            };
            btnAddEquipment.Click += BtnAddEquipment_Click;

            var separator = new Label
            {
                Location = new Point(181, 8),
                Size = new Size(2, 34),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightGray
            };

            cmbFilterDepartment = new ComboBox
            {
                Location = new Point(194, 12),
                Width = 182,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Id"
            };
            cmbFilterDepartment.Items.Add(new ConstructionDepartment { Id = 0, Name = "Все управления" });
            cmbFilterDepartment.Items.AddRange(repository.GetDepartments().ToArray());
            cmbFilterDepartment.SelectedIndex = 0;
            cmbFilterDepartment.SelectedIndexChanged += (s, e) => LoadEquipment();

            cmbFilterType = new ComboBox
            {
                Location = new Point(390, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterType.Items.Add("Все типы");
            cmbFilterType.Items.Add("Кран");
            cmbFilterType.Items.Add("Экскаватор");
            cmbFilterType.Items.Add("Бульдозер");
            cmbFilterType.Items.Add("Копёр");
            cmbFilterType.Items.Add("Грузовик");
            cmbFilterType.Items.Add("Самосвал");
            cmbFilterType.Items.Add("Бетономешалка");
            cmbFilterType.SelectedIndex = 0;
            cmbFilterType.SelectedIndexChanged += (s, e) => LoadEquipment();

            topPanel.Controls.Add(btnAddEquipment);
            topPanel.Controls.Add(separator);
            topPanel.Controls.Add(cmbFilterDepartment);
            topPanel.Controls.Add(cmbFilterType);

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(equipmentGrid);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);
        }

        // ✅ ОБНОВЛЕНИЕ пунктов меню при открытии
        private void ContextMenuStrip_Opening(object? sender, CancelEventArgs e)
        {
            var equipment = GetSelectedEquipment();

            // ✅ Обновляем пункт "Освободить технику"
            var freeMenuItem = contextMenuStrip.Items.Find("FreeMenuItem", false).FirstOrDefault() as ToolStripMenuItem;
            if (freeMenuItem != null)
            {
                // Показываем только если техника назначена на объект
                freeMenuItem.Visible = equipment?.CurrentObjectId != null && equipment.CurrentObjectId != 0;
            }

            // ✅ Обновляем пункт неисправности
            var brokenMenuItem = contextMenuStrip.Items.Find("BrokenMenuItem", false).FirstOrDefault() as ToolStripMenuItem;
            if (brokenMenuItem != null && equipment != null)
            {
                brokenMenuItem.Text = equipment.IsBroken ? "Объявить о исправности" : "Объявить о неисправности";
            }
        }

        private Equipment GetSelectedEquipment()
        {
            if (equipmentGrid.CurrentRow?.DataBoundItem == null) return null;
            int id = (int)equipmentGrid.CurrentRow.Cells["Id"].Value;
            return repository.GetEquipment(id);
        }

        private void SetupEquipmentGrid()
        {
            equipmentGrid.Columns.Clear();

            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Название",
                DataPropertyName = "Name",
                FillWeight = 1
            });
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Type",
                HeaderText = "Тип",
                DataPropertyName = "Type",
                FillWeight = 1
            });
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InventoryNumber",
                HeaderText = "Инв. номер",
                DataPropertyName = "InventoryNumber",
                FillWeight = 1
            });
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Department",
                HeaderText = "Управление",
                DataPropertyName = "DepartmentName",
                FillWeight = 1
            });
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Object",
                HeaderText = "Объект",
                DataPropertyName = "ObjectName",
                FillWeight = 1
            });
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Статус",
                DataPropertyName = "Status",
                FillWeight = 1
            });
        }

        private void LoadEquipment()
        {
            var equipment = repository.GetEquipment().Select(e => new
            {
                e.Id,
                e.Name,
                Type = GetEquipmentTypeInRussian(e.Type),
                e.InventoryNumber,
                DepartmentName = repository.GetDepartment(e.DepartmentId)?.Name ?? "Неизвестно",
                ObjectName = e.CurrentObjectId != 0 ? repository.GetObject(e.CurrentObjectId)?.Name ?? "Не назначен" : "Не назначен",
                Status = GetCalculatedStatus(e)
            }).ToList();

            if (cmbFilterDepartment.SelectedIndex > 0 && cmbFilterDepartment.SelectedItem is ConstructionDepartment selectedDept)
            {
                if (selectedDept.Id > 0)
                {
                    equipment = equipment.Where(e => e.DepartmentName == selectedDept.Name).ToList();
                }
            }

            if (cmbFilterType.SelectedIndex > 0)
            {
                var selectedType = cmbFilterType.SelectedItem.ToString();
                equipment = equipment.Where(e => e.Type == selectedType).ToList();
            }

            equipmentGrid.DataSource = equipment;
        }

        private string GetCalculatedStatus(Equipment e)
        {
            if (e.IsBroken)
                return "Неисправна";

            if (e.CurrentObjectId == 0 || e.AssignedDate == null)
                return "Свободна";

            var now = DateTime.Today;
            var assignedDate = e.AssignedDate.Value.Date;

            if (now >= assignedDate)
                return "В работе";

            return "Свободна";
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

        private EquipmentType GetEquipmentTypeFromRussian(string russianName)
        {
            switch (russianName)
            {
                case "Кран": return EquipmentType.Crane;
                case "Экскаватор": return EquipmentType.Excavator;
                case "Бульдозер": return EquipmentType.Bulldozer;
                case "Копёр": return EquipmentType.PileDriver;
                case "Грузовик": return EquipmentType.Truck;
                case "Самосвал": return EquipmentType.DumpTruck;
                case "Бетономешалка": return EquipmentType.ConcreteMixer;
                default: return EquipmentType.Crane;
            }
        }

        private void AddRowNumbers(DataGridView grid, string columnName = "RowNumber")
        {
            if (!grid.Columns.Contains(columnName))
                return;

            for (int i = 0; i < grid.Rows.Count; i++)
            {
                if (grid.Rows[i].IsNewRow)
                    continue;

                if (grid.Rows[i].Cells[columnName] != null)
                {
                    grid.Rows[i].Cells[columnName].Value = (i + 1).ToString();
                }
            }
        }

        private int GetSelectedEquipmentId()
        {
            if (equipmentGrid.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Выберите технику.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return -1;
            }
            return (int)equipmentGrid.CurrentRow.Cells["Id"].Value;
        }

        private void BtnAddEquipment_Click(object sender, EventArgs e)
        {
            var form = new AddEquipmentForm(repository);
            if (form.ShowDialog() == DialogResult.OK) LoadEquipment();
        }

        private void ContextMenu_Edit_Click(object? sender, EventArgs e)
        {
            int equipmentId = GetSelectedEquipmentId();
            if (equipmentId < 0) return;
            var equipment = repository.GetEquipment(equipmentId);
            if (equipment == null) return;
            var form = new AddEquipmentForm(repository, equipment);
            if (form.ShowDialog() == DialogResult.OK) LoadEquipment();
        }

        private void ContextMenu_Assign_Click(object? sender, EventArgs e)
        {
            int equipmentId = GetSelectedEquipmentId();
            if (equipmentId < 0) return;
            var form = new AssignEquipmentForm(repository, equipmentId);
            if (form.ShowDialog() == DialogResult.OK) LoadEquipment();
        }

        // ✅ НОВЫЙ МЕТОД: Освободить технику
        private void ContextMenu_FreeEquipment_Click(object? sender, EventArgs e)
        {
            int equipmentId = GetSelectedEquipmentId();
            if (equipmentId < 0) return;

            var equipment = repository.GetEquipment(equipmentId);
            if (equipment == null) return;

            string equipmentName = equipmentGrid.CurrentRow?.Cells["Name"].Value?.ToString() ?? "техника";
            string objectName = equipment.CurrentObjectId != 0 ? repository.GetObject(equipment.CurrentObjectId)?.Name ?? "объекта" : "объекта";

            if (MessageBox.Show($"Освободить технику \"{equipmentName}\" от объекта \"{objectName}\"?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // ✅ Сбрасываем привязку к объекту
                equipment.CurrentObjectId = 0;
                equipment.AssignedDate = null;
                // ✅ Статус пересчитывается автоматически
                equipment.Status = GetCalculatedStatus(equipment);

                repository.UpdateEquipment(equipment);
                LoadEquipment();
                MessageBox.Show("Техника освобождена.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ContextMenu_ToggleBroken_Click(object? sender, EventArgs e)
        {
            int equipmentId = GetSelectedEquipmentId();
            if (equipmentId < 0) return;

            var equipment = repository.GetEquipment(equipmentId);
            if (equipment == null) return;

            string equipmentName = equipmentGrid.CurrentRow?.Cells["Name"].Value?.ToString() ?? "техника";

            if (equipment.IsBroken)
            {
                if (MessageBox.Show($"Объявить технику \"{equipmentName}\" исправной?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    equipment.IsBroken = false;
                    equipment.Status = GetCalculatedStatus(equipment);
                    repository.UpdateEquipment(equipment);
                    LoadEquipment();
                    MessageBox.Show("Техника объявлена исправной.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                if (MessageBox.Show($"Объявить технику \"{equipmentName}\" неисправной?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    equipment.IsBroken = true;
                    equipment.Status = "Неисправна";
                    repository.UpdateEquipment(equipment);
                    LoadEquipment();
                    MessageBox.Show("Техника объявлена неисправной.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ContextMenu_Delete_Click(object? sender, EventArgs e)
        {
            int equipmentId = GetSelectedEquipmentId();
            if (equipmentId < 0) return;
            string equipmentName = equipmentGrid.CurrentRow?.Cells["Name"].Value?.ToString() ?? "техника";
            if (MessageBox.Show($"Удалить технику \"{equipmentName}\"?", "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                repository.DeleteEquipment(equipmentId);
                LoadEquipment();
                MessageBox.Show("Техника удалена.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    // Форма добавления/редактирования техники (БЕЗ периода)
    public class AddEquipmentForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly Equipment editingEquipment;
        private TextBox txtName;
        private ComboBox cmbType;
        private ComboBox cmbDepartment;

        private const int LABEL_WIDTH = 100;
        private const int FIELD_X = 120;
        private const int FIELD_WIDTH = 280;
        private const int ROW_HEIGHT = 35;
        private const int START_Y = 20;

        public AddEquipmentForm(IConstructionRepository repo) : this(repo, null) { }
        public AddEquipmentForm(IConstructionRepository repo, Equipment equipmentToEdit)
        {
            repository = repo;
            editingEquipment = equipmentToEdit;
            InitializeForm();
            if (editingEquipment != null) FillFieldsForEdit();
        }

        private void InitializeForm()
        {
            this.Text = editingEquipment == null ? "Добавить технику" : "Редактировать технику";
            this.Size = new Size(450, 225);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = START_Y;

            Controls.Add(new Label { Text = "* Название:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            Controls.Add(txtName = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH });
            yPos += ROW_HEIGHT;

            Controls.Add(new Label { Text = "Тип:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            Controls.Add(cmbType = new ComboBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH, DropDownStyle = ComboBoxStyle.DropDownList });
            cmbType.Items.Add("Кран");
            cmbType.Items.Add("Экскаватор");
            cmbType.Items.Add("Бульдозер");
            cmbType.Items.Add("Копёр");
            cmbType.Items.Add("Грузовик");
            cmbType.Items.Add("Самосвал");
            cmbType.Items.Add("Бетономешалка");
            cmbType.SelectedIndex = 0;
            yPos += ROW_HEIGHT;

            Controls.Add(new Label { Text = "* Управление:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            Controls.Add(cmbDepartment = new ComboBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH, DropDownStyle = ComboBoxStyle.DropDownList });
            cmbDepartment.DataSource = repository.GetDepartments();
            cmbDepartment.DisplayMember = "Name";
            cmbDepartment.ValueMember = "Id";
            yPos += ROW_HEIGHT + 10;

            var btnSave = new Button { Text = "Сохранить", Location = new Point(FIELD_X, yPos), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(FIELD_X + 120, yPos), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSave); Controls.Add(btnCancel);
        }

        private void FillFieldsForEdit()
        {
            txtName.Text = editingEquipment.Name;
            cmbType.SelectedItem = GetEquipmentTypeInRussian(editingEquipment.Type);
            var department = repository.GetDepartment(editingEquipment.DepartmentId);
            if (department != null) cmbDepartment.SelectedValue = department.Id;
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
                default: return "Кран";
            }
        }

        private EquipmentType GetEquipmentTypeFromRussian(string russianName)
        {
            switch (russianName)
            {
                case "Кран": return EquipmentType.Crane;
                case "Экскаватор": return EquipmentType.Excavator;
                case "Бульдозер": return EquipmentType.Bulldozer;
                case "Копёр": return EquipmentType.PileDriver;
                case "Грузовик": return EquipmentType.Truck;
                case "Самосвал": return EquipmentType.DumpTruck;
                case "Бетономешалка": return EquipmentType.ConcreteMixer;
                default: return EquipmentType.Crane;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            bool nameEmpty = string.IsNullOrWhiteSpace(txtName.Text);
            bool departmentEmpty = cmbDepartment.SelectedValue == null;

            // Общее сообщение выводится только когда не заполнено ни одно обязательное поле
            if (nameEmpty && departmentEmpty)
            {
                MessageBox.Show("Заполните обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Если заполнено частично, указываем конкретное незаполненное поле
            if (nameEmpty)
            {
                MessageBox.Show("Введите название техники!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            // Проверка длины названия техники: минимум 3 символа
            if (txtName.Text.Trim().Length < 3)
            {
                MessageBox.Show("Название техники должно содержать не менее 3 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (departmentEmpty)
            {
                MessageBox.Show("Выберите управление!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbDepartment.Focus();
                return;
            }

            string inventoryNumber = $"INV-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}";
            if (editingEquipment == null)
            {
                repository.AddEquipment(new Equipment
                {
                    Name = txtName.Text.Trim(),
                    Type = GetEquipmentTypeFromRussian(cmbType.SelectedItem.ToString()),
                    InventoryNumber = inventoryNumber,
                    DepartmentId = (int)cmbDepartment.SelectedValue,
                    Status = "Свободна",
                    IsBroken = false
                });
            }
            else
            {
                editingEquipment.Name = txtName.Text.Trim();
                editingEquipment.Type = GetEquipmentTypeFromRussian(cmbType.SelectedItem.ToString());
                editingEquipment.DepartmentId = (int)cmbDepartment.SelectedValue;
                repository.UpdateEquipment(editingEquipment);
            }
            MessageBox.Show("Техника успешно сохранена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK; Close();
        }
    }

    // Форма назначения техники на объект (С периодом)
    public class AssignEquipmentForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int equipmentId;
        private ComboBox cmbObject;
        private DateTimePicker dtpStartDate, dtpEndDate;

        private const int LABEL_WIDTH = 100;
        private const int FIELD_X = 120;
        private const int FIELD_WIDTH = 280;
        private const int ROW_HEIGHT = 35;
        private const int START_Y = 20;

        public AssignEquipmentForm(IConstructionRepository repo, int equipId)
        {
            repository = repo;
            equipmentId = equipId;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Назначить технику на объект";
            this.Size = new Size(450, 190);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = START_Y;

            var lblObject = new Label { Text = "* Объект:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) };
            cmbObject = new ComboBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbObject.DataSource = repository.GetObjects();
            cmbObject.DisplayMember = "Name";
            cmbObject.ValueMember = "Id";
            Controls.Add(lblObject); Controls.Add(cmbObject);
            yPos += ROW_HEIGHT;

            var lblPeriod = new Label
            {
                Text = "Период:",
                Location = new Point(10, yPos),
                Size = new Size(LABEL_WIDTH, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblFrom = new Label
            {
                Text = "с",
                Location = new Point(FIELD_X, yPos),
                Size = new Size(20, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };

            dtpStartDate = new DateTimePicker
            {
                Location = new Point(FIELD_X + 25, yPos),
                Width = 110,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };

            var lblTo = new Label
            {
                Text = "по",
                Location = new Point(FIELD_X + 140, yPos),
                Size = new Size(25, 23),
                TextAlign = ContentAlignment.MiddleCenter
            };

            dtpEndDate = new DateTimePicker
            {
                Location = new Point(FIELD_X + 170, yPos),
                Width = 110,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(1)
            };

            Controls.Add(lblPeriod); Controls.Add(lblFrom); Controls.Add(dtpStartDate);
            Controls.Add(lblTo); Controls.Add(dtpEndDate);
            yPos += ROW_HEIGHT + 10;

            var btnSave = new Button { Text = "Назначить", Location = new Point(FIELD_X, yPos), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(FIELD_X + 120, yPos), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSave); Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbObject.SelectedValue == null)
            {
                MessageBox.Show("Выберите объект!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var equipment = repository.GetEquipment(equipmentId);
            if (equipment != null)
            {
                equipment.CurrentObjectId = (int)cmbObject.SelectedValue;
                equipment.AssignedDate = dtpStartDate.Value;
                equipment.Status = "В работе";
                repository.UpdateEquipment(equipment);
            }
            MessageBox.Show("Техника назначена на объект!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK; Close();
        }
    }
}