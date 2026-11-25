using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public class BrigadesManagementForm : Form
    {
        private readonly IConstructionRepository repository;
        private DataGridView brigadesGrid;
        private Button btnAddBrigade;
        private ContextMenuStrip contextMenuStrip;

        public BrigadesManagementForm(IConstructionRepository repo)
        {
            repository = repo;
            InitializeForm();
            LoadBrigades();
        }

        private void InitializeForm()
        {
            this.Text = "Управление бригадами";
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            brigadesGrid = new DataGridView
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

            // ✅ ИСПРАВЛЕНИЕ: нумерация после полной загрузки данных
            brigadesGrid.DataBindingComplete += (s, e) => AddRowNumbers(brigadesGrid);

            SetupBrigadesGrid();

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Редактировать", null, ContextMenu_Edit_Click);
            contextMenuStrip.Items.Add("Состав бригады", null, ContextMenu_ViewWorkers_Click);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add("Удалить", null, ContextMenu_Delete_Click);

            brigadesGrid.ContextMenuStrip = contextMenuStrip;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke
            };

            btnAddBrigade = new Button
            {
                Text = "Добавить бригаду",
                Location = new Point(10, 10),
                Size = new Size(160, 30)
            };
            btnAddBrigade.Click += BtnAddBrigade_Click;

            topPanel.Controls.Add(btnAddBrigade);
            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(brigadesGrid);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);
        }

        private void SetupBrigadesGrid()
        {
            brigadesGrid.Columns.Clear();

            // === КОЛОНКА "№" (МАЛЕНЬКАЯ: фиксированная ширина 25px) ===
            brigadesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "№",
                Width = 25,  // ← ФИКСИРОВАННАЯ МАЛЕНЬКАЯ ШИРИНА
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,  // ← НЕ участвовать в AutoSize
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,  // ← По левому краю
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),    // ← Обычный шрифт
                    BackColor = Color.White                                // ← Белый фон
                }
            });

            // Скрытый реальный ID
            brigadesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            // Остальные колонки (ОДИНАКОВОГО размера: FillWeight = 1)
            brigadesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Название",
                DataPropertyName = "Name",
                FillWeight = 1
            });
            brigadesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Leader",
                HeaderText = "Бригадир",
                DataPropertyName = "LeaderName",
                FillWeight = 1
            });
            brigadesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "WorkersCount",
                HeaderText = "Кол-во рабочих",
                DataPropertyName = "WorkersCount",
                FillWeight = 1
            });
            brigadesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Object",
                HeaderText = "Объект",
                DataPropertyName = "ObjectName",
                FillWeight = 1
            });
            brigadesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "WorkType",
                HeaderText = "Вид работ",
                DataPropertyName = "WorkType",
                FillWeight = 1
            });
        }

        private void LoadBrigades()
        {
            var brigades = repository.GetBrigades().Select(b => new
            {
                b.Id,
                b.Name,
                LeaderName = b.LeaderId != 0
                    ? repository.GetEmployee(b.LeaderId)?.FullName ?? "Не назначен"
                    : "Не назначен",
                WorkersCount = repository.GetWorkersByBrigade(b.Id).Count,
                ObjectName = b.CurrentObjectId != 0
                    ? repository.GetObject(b.CurrentObjectId)?.Name ?? "Не назначен"
                    : "Не назначен",
                WorkType = b.CurrentWorkType.HasValue
                    ? GetWorkTypeInRussian(b.CurrentWorkType.Value)
                    : "Не назначен"
            }).ToList();

            brigadesGrid.DataSource = brigades;
            // ✅ УБРАНО: AddRowNumbers вызывается автоматически через DataBindingComplete
        }

        // === ВСПОМОГАТЕЛЬНЫЙ МЕТОД: добавление порядковых номеров ===
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

        private string GetWorkTypeInRussian(WorkType type)
        {
            switch (type)
            {
                case WorkType.Foundation: return "Фундаментные работы";
                case WorkType.Brickwork: return "Кирпичная кладка";
                case WorkType.Plumbing: return "Сантехнические работы";
                case WorkType.Electrical: return "Электромонтажные работы";
                case WorkType.Finishing: return "Отделочные работы";
                case WorkType.Roadwork: return "Дорожные работы";
                default: return "Неизвестный вид работ";
            }
        }

        private int GetSelectedBrigadeId()
        {
            if (brigadesGrid.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Выберите бригаду.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return -1;
            }
            return (int)brigadesGrid.CurrentRow.Cells["Id"].Value;
        }

        private void BtnAddBrigade_Click(object sender, EventArgs e)
        {
            var form = new AddBrigadeForm(repository);
            if (form.ShowDialog() == DialogResult.OK) LoadBrigades();
        }

        private void ContextMenu_Edit_Click(object? sender, EventArgs e)
        {
            int brigadeId = GetSelectedBrigadeId();
            if (brigadeId < 0) return;
            var brigade = repository.GetBrigade(brigadeId);
            if (brigade == null) return;
            var form = new AddBrigadeForm(repository, brigade);
            if (form.ShowDialog() == DialogResult.OK) LoadBrigades();
        }

        private void ContextMenu_Delete_Click(object? sender, EventArgs e)
        {
            int brigadeId = GetSelectedBrigadeId();
            if (brigadeId < 0) return;
            string brigadeName = brigadesGrid.CurrentRow?.Cells["Name"].Value?.ToString() ?? "бригада";
            if (MessageBox.Show($"Удалить бригаду \"{brigadeName}\"?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                repository.DeleteBrigade(brigadeId);
                LoadBrigades();
                MessageBox.Show("Бригада удалена.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ContextMenu_ViewWorkers_Click(object? sender, EventArgs e)
        {
            int brigadeId = GetSelectedBrigadeId();
            if (brigadeId < 0) return;
            var form = new BrigadeWorkersForm(repository, brigadeId);
            form.ShowDialog();
        }
    }

    public class AddBrigadeForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly Brigade editingBrigade;
        private TextBox txtName;
        private ComboBox cmbLeader;
        private ComboBox cmbObject;
        private ComboBox cmbWorkType;

        public AddBrigadeForm(IConstructionRepository repo) : this(repo, null) { }
        public AddBrigadeForm(IConstructionRepository repo, Brigade brigadeToEdit)
        {
            repository = repo;
            editingBrigade = brigadeToEdit;
            InitializeForm();
            if (editingBrigade != null) FillFieldsForEdit();
        }

        private void InitializeForm()
        {
            this.Text = editingBrigade == null ? "Добавить бригаду" : "Редактировать бригаду";
            this.Size = new Size(450, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;
            Controls.Add(new Label { Text = "* Название:", Location = new Point(10, yPos), Size = new Size(100, 20) });
            Controls.Add(txtName = new TextBox { Location = new Point(120, yPos), Width = 280 }); yPos += 35;

            Controls.Add(new Label { Text = "* Бригадир:", Location = new Point(10, yPos), Size = new Size(100, 20) });
            Controls.Add(cmbLeader = new ComboBox { Location = new Point(120, yPos), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList }); yPos += 35;
            var leaders = repository.GetEmployees().Where(e =>
                e.Position == Position.Foreman ||
                e.Position == Position.Master ||
                e.Position == Position.SiteManager
            ).ToList();
            cmbLeader.DataSource = leaders;
            cmbLeader.DisplayMember = "FullName";
            cmbLeader.ValueMember = "Id";

            Controls.Add(new Label { Text = "Объект:", Location = new Point(10, yPos), Size = new Size(100, 20) });
            Controls.Add(cmbObject = new ComboBox { Location = new Point(120, yPos), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList }); yPos += 35;
            cmbObject.DataSource = repository.GetObjects();
            cmbObject.DisplayMember = "Name";
            cmbObject.ValueMember = "Id";

            Controls.Add(new Label { Text = "Вид работ:", Location = new Point(10, yPos), Size = new Size(100, 20) });
            Controls.Add(cmbWorkType = new ComboBox { Location = new Point(120, yPos), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList }); yPos += 45;
            cmbWorkType.Items.Add("Фундаментные работы");
            cmbWorkType.Items.Add("Кирпичная кладка");
            cmbWorkType.Items.Add("Сантехнические работы");
            cmbWorkType.Items.Add("Электромонтажные работы");
            cmbWorkType.Items.Add("Отделочные работы");
            cmbWorkType.Items.Add("Дорожные работы");

            var btnSave = new Button { Text = "Сохранить", Location = new Point(120, yPos), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(240, yPos), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
        }

        private void FillFieldsForEdit()
        {
            txtName.Text = editingBrigade.Name;
            if (editingBrigade.LeaderId != 0 && cmbLeader.Items.Count > 0) cmbLeader.SelectedValue = editingBrigade.LeaderId;
            else cmbLeader.SelectedIndex = -1;
            if (editingBrigade.CurrentObjectId != 0 && cmbObject.Items.Count > 0) cmbObject.SelectedValue = editingBrigade.CurrentObjectId;
            else cmbObject.SelectedIndex = -1;
            if (editingBrigade.CurrentWorkType.HasValue) cmbWorkType.SelectedItem = GetWorkTypeInRussian(editingBrigade.CurrentWorkType.Value);
            else cmbWorkType.SelectedIndex = -1;
        }

                private void BtnSave_Click(object sender, EventArgs e)
        {
            bool nameEmpty = string.IsNullOrWhiteSpace(txtName.Text);
            bool leaderEmpty = cmbLeader.SelectedValue == null;

            // Общее сообщение выводится только когда не заполнено ни одно обязательное поле
            if (nameEmpty && leaderEmpty)
            {
                MessageBox.Show("Заполните обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Если заполнено частично, указываем конкретное незаполненное поле
            if (nameEmpty)
            {
                MessageBox.Show("Введите название бригады!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            // Проверка длины названия бригады: минимум 2 символа
            if (txtName.Text.Trim().Length < 2)
            {
                MessageBox.Show("Название бригады должно содержать не менее 2 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (leaderEmpty)
            {
                MessageBox.Show("Выберите бригадира!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbLeader.Focus();
                return;
            }

            int leaderId = (int)cmbLeader.SelectedValue;
            int objectId = cmbObject.SelectedValue != null ? (int)cmbObject.SelectedValue : 0;
            var workTypeMap = new System.Collections.Generic.Dictionary<string, WorkType>
            {
                { "Фундаментные работы", WorkType.Foundation },
                { "Кирпичная кладка", WorkType.Brickwork },
                { "Сантехнические работы", WorkType.Plumbing },
                { "Электромонтажные работы", WorkType.Electrical },
                { "Отделочные работы", WorkType.Finishing },
                { "Дорожные работы", WorkType.Roadwork }
            };
            WorkType? workType = null;
            if (cmbWorkType.SelectedItem != null && workTypeMap.ContainsKey(cmbWorkType.SelectedItem.ToString()))
                workType = workTypeMap[cmbWorkType.SelectedItem.ToString()];
            if (editingBrigade == null)
            {
                repository.AddBrigade(new Brigade { Name = txtName.Text.Trim(), LeaderId = leaderId, CurrentObjectId = objectId, CurrentWorkType = workType });
            }
            else
            {
                editingBrigade.Name = txtName.Text.Trim();
                editingBrigade.LeaderId = leaderId;
                editingBrigade.CurrentObjectId = objectId;
                editingBrigade.CurrentWorkType = workType;
                repository.UpdateBrigade(editingBrigade);
            }
            MessageBox.Show("Бригада успешно сохранена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK; Close();
        }

        private string GetWorkTypeInRussian(WorkType type)
        {
            switch (type)
            {
                case WorkType.Foundation: return "Фундаментные работы";
                case WorkType.Brickwork: return "Кирпичная кладка";
                case WorkType.Plumbing: return "Сантехнические работы";
                case WorkType.Electrical: return "Электромонтажные работы";
                case WorkType.Finishing: return "Отделочные работы";
                case WorkType.Roadwork: return "Дорожные работы";
                default: return "Неизвестный вид работ";
            }
        }
    }

    // === ФОРМА: ДОБАВЛЕНИЕ РАБОЧЕГО В БРИГАДУ ===
    public class AddWorkerToBrigadeForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int brigadeId;
        private ComboBox cmbWorker;
        private Label lblSpecialty;

        public AddWorkerToBrigadeForm(IConstructionRepository repo, int brigadeId)
        {
            repository = repo;
            this.brigadeId = brigadeId;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Добавить рабочего в бригаду";
            this.Size = new Size(450, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;
            Controls.Add(new Label { Text = "* Рабочий:", Location = new Point(10, yPos), Size = new Size(100, 20) });
            cmbWorker = new ComboBox { Location = new Point(120, yPos), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbWorker.DisplayMember = "FullName"; cmbWorker.ValueMember = "Id";
            var allWorkers = repository.GetWorkers() ?? new System.Collections.Generic.List<Worker>();
            cmbWorker.DataSource = allWorkers;
            Controls.Add(cmbWorker); yPos += 35;

            lblSpecialty = new Label { Text = "Специальность: -", Location = new Point(10, yPos), Size = new Size(300, 20) };
            cmbWorker.SelectedIndexChanged += (s, e) => { if (cmbWorker.SelectedItem is Worker w) lblSpecialty.Text = $"Специальность: {GetSpecialtyInRussian(w.Specialty)}"; };
            Controls.Add(lblSpecialty); yPos += 45;

            var btnSave = new Button { Text = "Добавить", Location = new Point(120, yPos), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(240, yPos), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSave); Controls.Add(btnCancel);
            if (cmbWorker.SelectedItem is Worker initialWorker) lblSpecialty.Text = $"Специальность: {GetSpecialtyInRussian(initialWorker.Specialty)}";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbWorker.SelectedValue == null) { MessageBox.Show("Выберите рабочего!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            int workerId = (int)cmbWorker.SelectedValue;
            var worker = repository.GetWorker(workerId);
            if (worker != null)
            {
                worker.BrigadeId = brigadeId;
                repository.UpdateWorker(worker);
                MessageBox.Show($"Рабочий \"{worker.FullName}\" добавлен в бригаду!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK; Close();
            }
        }

        private string GetSpecialtyInRussian(WorkerSpecialty specialty)
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
                default: return "Неизвестная специальность";
            }
        }
    }

    // === ФОРМА: СОСТАВ БРИГАДЫ С КОНТЕКСТНЫМ МЕНЮ ===
    public class BrigadeWorkersForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int brigadeId;
        private DataGridView workersGrid;
        private Button btnAddWorker;
        private ContextMenuStrip workerContextMenu;

        public BrigadeWorkersForm(IConstructionRepository repo, int brigadeId)
        {
            repository = repo;
            this.brigadeId = brigadeId;
            InitializeForm();
            LoadWorkers();
        }

        private void InitializeForm()
        {
            var brigade = repository.GetBrigade(brigadeId);
            this.Text = $"Состав бригады: {brigade?.Name ?? "Неизвестно"}";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            workersGrid = new DataGridView
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

            // ✅ ИСПРАВЛЕНИЕ: нумерация после полной загрузки данных
            workersGrid.DataBindingComplete += (s, e) => AddRowNumbers(workersGrid);

            SetupWorkersGrid();

            workerContextMenu = new ContextMenuStrip();
            workerContextMenu.Items.Add("Редактировать рабочего", null, ContextMenu_EditWorker_Click);
            workerContextMenu.Items.Add(new ToolStripSeparator());
            workerContextMenu.Items.Add("Убрать из бригады", null, ContextMenu_RemoveWorker_Click);
            workersGrid.ContextMenuStrip = workerContextMenu;

            btnAddWorker = new Button { Text = "Добавить рабочего", Location = new Point(10, 10), Size = new Size(150, 30) };
            btnAddWorker.Click += BtnAddWorker_Click;

            var buttonPanel = new Panel { Dock = DockStyle.Top, Height = 50 };
            buttonPanel.Controls.Add(btnAddWorker);

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(workersGrid);
            mainPanel.Controls.Add(buttonPanel);
            this.Controls.Add(mainPanel);
        }

        private void SetupWorkersGrid()
        {
            workersGrid.Columns.Clear();

            // === КОЛОНКА "№" (МАЛЕНЬКАЯ: фиксированная ширина 25px) ===
            workersGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            // Скрытый реальный ID
            workersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "WorkerId", HeaderText = "ID", DataPropertyName = "WorkerId", Visible = false });

            // Остальные колонки (ОДИНАКОВОГО размера: FillWeight = 1)
            workersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ФИО", DataPropertyName = "FullName", FillWeight = 1 });
            workersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Специальность", DataPropertyName = "Specialty", FillWeight = 1 });
            workersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Разряд", DataPropertyName = "Qualification", FillWeight = 1 });
            workersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Стаж", DataPropertyName = "Experience", FillWeight = 1 });
            workersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Допуск", DataPropertyName = "SafetyCert", FillWeight = 1 });
        }

        private void LoadWorkers()
        {
            var workers = repository.GetWorkersByBrigade(brigadeId).Select(w => new
            {
                WorkerId = w.Id,
                w.FullName,
                Specialty = GetSpecialtyInRussian(w.Specialty),
                Qualification = w.QualificationLevel?.ToString() ?? "-",
                Experience = w.HireDate.HasValue ? $"{(DateTime.Now.Year - w.HireDate.Value.Year)} лет" : "-",
                SafetyCert = w.HasSafetyCertification ? "Да" : "Нет"
            }).ToList();

            workersGrid.DataSource = workers;
            // AddRowNumbers вызывается автоматически через DataBindingComplete
        }

        // === ВСПОМОГАТЕЛЬНЫЙ МЕТОД: добавление порядковых номеров ===
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

        private string GetSpecialtyInRussian(WorkerSpecialty specialty)
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
                default: return "Неизвестная специальность";
            }
        }

        private int GetSelectedWorkerId()
        {
            if (workersGrid.CurrentRow?.DataBoundItem == null) { MessageBox.Show("Выберите рабочего.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information); return -1; }
            return (int)workersGrid.CurrentRow.Cells["WorkerId"].Value;
        }

        private void BtnAddWorker_Click(object sender, EventArgs e)
        {
            var form = new AddWorkerToBrigadeForm(repository, brigadeId);
            if (form.ShowDialog() == DialogResult.OK) LoadWorkers();
        }

        private void ContextMenu_EditWorker_Click(object? sender, EventArgs e)
        {
            int workerId = GetSelectedWorkerId(); if (workerId < 0) return;
            var worker = repository.GetWorker(workerId); if (worker == null) return;
            var form = new AddWorkerForm(repository, worker);
            if (form.ShowDialog() == DialogResult.OK) LoadWorkers();
        }

        private void ContextMenu_RemoveWorker_Click(object? sender, EventArgs e)
        {
            int workerId = GetSelectedWorkerId(); if (workerId < 0) return;
            var worker = repository.GetWorker(workerId); if (worker == null) return;
            if (MessageBox.Show($"Убрать рабочего \"{worker.FullName}\" из бригады?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                worker.BrigadeId = 0;
                repository.UpdateWorker(worker);
                LoadWorkers();
                MessageBox.Show($"Рабочий \"{worker.FullName}\" убран из бригады.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    // === ФОРМА: ДОБАВЛЕНИЕ/РЕДАКТИРОВАНИЕ РАБОЧЕГО ===
    public class AddWorkerForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly Worker editingWorker;
        private TextBox txtFullName;
        private ComboBox cmbSpecialty;
        private TextBox txtQualification;
        private DateTimePicker dtpHireDate;
        private CheckBox chkSafetyCert;

        public AddWorkerForm(IConstructionRepository repo) : this(repo, null) { }
        public AddWorkerForm(IConstructionRepository repo, Worker workerToEdit)
        {
            repository = repo;
            editingWorker = workerToEdit;
            InitializeForm();
            if (editingWorker != null) FillFieldsForEdit();
        }

        private void InitializeForm()
        {
            this.Text = editingWorker == null ? "Добавить рабочего" : "Редактировать рабочего";
            this.Size = new Size(450, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;

            Controls.Add(new Label { Text = "* ФИО:", Location = new Point(10, yPos), Size = new Size(100, 20) });
            Controls.Add(txtFullName = new TextBox { Location = new Point(120, yPos), Width = 280 }); yPos += 35;

            Controls.Add(new Label { Text = "Специальность:", Location = new Point(10, yPos), Size = new Size(100, 20) });
            Controls.Add(cmbSpecialty = new ComboBox { Location = new Point(120, yPos), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList });

            // ✅ DataSource — список русских названий, а не enum.
            // Это гарантирует, что и в списке, и в выбранном значении будут русские названия.
            cmbSpecialty.DataSource = Enum.GetValues(typeof(WorkerSpecialty))
                .Cast<WorkerSpecialty>()
                .Select(s => GetSpecialtyInRussian(s))
                .ToList();

            yPos += 35;

            Controls.Add(new Label { Text = "Разряд:", Location = new Point(10, yPos), Size = new Size(100, 20) });
            Controls.Add(txtQualification = new TextBox { Location = new Point(120, yPos), Width = 100 }); yPos += 35;

            Controls.Add(new Label { Text = "Дата приёма:", Location = new Point(10, yPos), Size = new Size(100, 20) });
            Controls.Add(dtpHireDate = new DateTimePicker { Location = new Point(120, yPos), Width = 150, Value = DateTime.Now }); yPos += 35;

            Controls.Add(chkSafetyCert = new CheckBox { Text = "Есть допуск по безопасности", Location = new Point(120, yPos), AutoSize = true }); yPos += 45;

            var btnSave = new Button { Text = "Сохранить", Location = new Point(120, yPos), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(240, yPos), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSave); Controls.Add(btnCancel);
        }

        private void FillFieldsForEdit()
        {
            txtFullName.Text = editingWorker.FullName;

            // ✅ Устанавливаем выбранное значение через русское название,
            // так как DataSource теперь содержит строки, а не enum.
            cmbSpecialty.SelectedItem = GetSpecialtyInRussian(editingWorker.Specialty);

            txtQualification.Text = editingWorker.QualificationLevel ?? "";
            dtpHireDate.Value = editingWorker.HireDate ?? DateTime.Now;
            chkSafetyCert.Checked = editingWorker.HasSafetyCertification;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text)) { MessageBox.Show("Введите ФИО рабочего!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (editingWorker == null)
            {
                repository.AddWorker(new Worker { FullName = txtFullName.Text.Trim(), Specialty = (WorkerSpecialty)cmbSpecialty.SelectedItem, QualificationLevel = txtQualification.Text.Trim(), HireDate = dtpHireDate.Value, HasSafetyCertification = chkSafetyCert.Checked, BrigadeId = 0 });
            }
            else
            {
                editingWorker.FullName = txtFullName.Text.Trim();
                editingWorker.Specialty = (WorkerSpecialty)cmbSpecialty.SelectedItem;
                editingWorker.QualificationLevel = txtQualification.Text.Trim();
                editingWorker.HireDate = dtpHireDate.Value;
                editingWorker.HasSafetyCertification = chkSafetyCert.Checked;
                repository.UpdateWorker(editingWorker);
            }
            MessageBox.Show("Рабочий успешно сохранён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK; Close();
        }

        private string GetSpecialtyInRussian(WorkerSpecialty specialty)
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
                default: return "Неизвестная специальность";
            }
        }
    }
}