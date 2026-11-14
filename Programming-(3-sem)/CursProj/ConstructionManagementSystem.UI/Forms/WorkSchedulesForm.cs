using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public class WorkSchedulesForm : Form
    {
        private readonly IConstructionRepository repository;
        private DataGridView schedulesGrid;
        private Button btnAddSchedule;
        private ContextMenuStrip contextMenuStrip;
        private ComboBox cmbFilterObject;
        private ComboBox cmbFilterWorkType;
        private CheckBox chkShowDelayed;

        public WorkSchedulesForm(IConstructionRepository repo)
        {
            repository = repo;
            InitializeForm();
            LoadSchedules();
        }

        private void InitializeForm()
        {
            this.Text = "Графики работ";
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            schedulesGrid = new DataGridView
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

            schedulesGrid.DataBindingComplete += (s, e) => AddRowNumbers(schedulesGrid);

            SetupSchedulesGrid();

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Редактировать", null, ContextMenu_Edit_Click);
            contextMenuStrip.Items.Add("Отчеты", null, ContextMenu_ViewReports_Click);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add("Удалить", null, ContextMenu_Delete_Click);

            schedulesGrid.ContextMenuStrip = contextMenuStrip;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke
            };

            btnAddSchedule = new Button
            {
                Text = "Добавить график",
                Location = new Point(10, 10),
                Size = new Size(160, 30)
            };
            btnAddSchedule.Click += BtnAddSchedule_Click;

            var separator = new Label
            {
                Location = new Point(181, 8),
                Size = new Size(2, 34),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightGray
            };

            cmbFilterObject = new ComboBox
            {
                Location = new Point(194, 12),
                Width = 182,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Id"
            };
            cmbFilterObject.Items.Add(new ConstructionObject { Id = 0, Name = "Все объекты" });
            cmbFilterObject.Items.AddRange(repository.GetObjects().ToArray());
            cmbFilterObject.SelectedIndex = 0;
            cmbFilterObject.SelectedIndexChanged += (s, e) => LoadSchedules();

            cmbFilterWorkType = new ComboBox
            {
                Location = new Point(390, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterWorkType.Items.Add("Все виды");
            cmbFilterWorkType.Items.Add("Фундамент");
            cmbFilterWorkType.Items.Add("Кладка");
            cmbFilterWorkType.Items.Add("Сантехника");
            cmbFilterWorkType.Items.Add("Электрика");
            cmbFilterWorkType.Items.Add("Отделка");
            cmbFilterWorkType.Items.Add("Дорожные работы");
            cmbFilterWorkType.SelectedIndex = 0;
            cmbFilterWorkType.SelectedIndexChanged += (s, e) => LoadSchedules();

            chkShowDelayed = new CheckBox
            {
                Text = "Показать задержки",
                Location = new Point(555, 15),
                AutoSize = true
            };
            chkShowDelayed.CheckedChanged += (s, e) => LoadSchedules();

            topPanel.Controls.Add(btnAddSchedule);
            topPanel.Controls.Add(separator);
            topPanel.Controls.Add(cmbFilterObject);
            topPanel.Controls.Add(cmbFilterWorkType);
            topPanel.Controls.Add(chkShowDelayed);

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(schedulesGrid);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);
        }

        private void SetupSchedulesGrid()
        {
            schedulesGrid.Columns.Clear();

            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Object",
                HeaderText = "Объект",
                DataPropertyName = "ObjectName",
                FillWeight = 1
            });
            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "WorkType",
                HeaderText = "Вид работ",
                DataPropertyName = "WorkType",
                FillWeight = 1
            });
            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Brigade",
                HeaderText = "Бригада",
                DataPropertyName = "BrigadeName",
                FillWeight = 1
            });
            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PlannedStart",
                HeaderText = "Начало",
                DataPropertyName = "PlannedStart",
                FillWeight = 1
            });
            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PlannedEnd",
                HeaderText = "Окончание",
                DataPropertyName = "PlannedEnd",
                FillWeight = 1
            });
            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ActualStart",
                HeaderText = "Факт. начало",
                DataPropertyName = "ActualStart",
                FillWeight = 1
            });
            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ActualEnd",
                HeaderText = "Факт. окончание",
                DataPropertyName = "ActualEnd",
                FillWeight = 1
            });
            schedulesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Статус",
                DataPropertyName = "Status",
                FillWeight = 1
            });
        }

        private void LoadSchedules()
        {
            var allSchedules = repository.GetWorkSchedules();

            if (cmbFilterObject.SelectedIndex > 0 && cmbFilterObject.SelectedItem is ConstructionObject selectedObject)
            {
                if (selectedObject.Id > 0)
                {
                    allSchedules = allSchedules.Where(s => s.ObjectId == selectedObject.Id).ToList();
                }
            }

            if (cmbFilterWorkType.SelectedIndex > 0)
            {
                var selectedWorkType = GetWorkTypeFromRussian(cmbFilterWorkType.SelectedItem.ToString());
                allSchedules = allSchedules.Where(s => s.WorkType == selectedWorkType).ToList();
            }

            if (chkShowDelayed.Checked)
            {
                allSchedules = allSchedules.Where(s => CalculateStatus(s) == WorkStatus.Delayed).ToList();
            }

            var schedules = allSchedules.Select(s => new
            {
                s.Id,
                ObjectName = repository.GetObject(s.ObjectId)?.Name ?? "Неизвестно",
                WorkType = GetWorkTypeInRussian(s.WorkType),
                BrigadeName = repository.GetBrigade(s.BrigadeId)?.Name ?? "Неизвестно",
                PlannedStart = s.PlannedStartDate.ToString("dd.MM.yyyy"),
                PlannedEnd = s.PlannedEndDate.ToString("dd.MM.yyyy"),
                ActualStart = s.ActualStartDate?.ToString("dd.MM.yyyy") ?? "-",
                ActualEnd = s.ActualEndDate?.ToString("dd.MM.yyyy") ?? "-",
                Status = GetStatusInRussian(CalculateStatus(s))
            }).ToList();

            schedulesGrid.DataSource = schedules;
        }

        private WorkStatus CalculateStatus(WorkSchedule s)
        {
            var now = DateTime.Today;

            if (s.ActualEndDate.HasValue)
            {
                if (now > s.PlannedEndDate.Date)
                {
                    if (now <= s.ActualEndDate.Value.Date)
                        return WorkStatus.Delayed;
                    else
                        return WorkStatus.Completed;
                }
                else
                {
                    if (now > s.ActualEndDate.Value.Date)
                        return WorkStatus.Completed;
                    else
                    {
                        if (s.ActualStartDate.HasValue)
                        {
                            if (now >= s.ActualStartDate.Value.Date)
                                return WorkStatus.InProgress;
                            else
                                return WorkStatus.Planned;
                        }
                        else
                        {
                            if (now >= s.PlannedStartDate.Date)
                                return WorkStatus.InProgress;
                            else
                                return WorkStatus.Planned;
                        }
                    }
                }
            }
            else
            {
                if (now > s.PlannedEndDate.Date)
                    return WorkStatus.Completed;
                else
                {
                    if (s.ActualStartDate.HasValue)
                    {
                        if (now >= s.ActualStartDate.Value.Date)
                            return WorkStatus.InProgress;
                        else
                            return WorkStatus.Planned;
                    }
                    else
                    {
                        if (now >= s.PlannedStartDate.Date)
                            return WorkStatus.InProgress;
                        else
                            return WorkStatus.Planned;
                    }
                }
            }
        }

        private string GetStatusInRussian(WorkStatus status)
        {
            switch (status)
            {
                case WorkStatus.Planned: return "Запланирован";
                case WorkStatus.InProgress: return "В процессе";
                case WorkStatus.Completed: return "Завершен";
                case WorkStatus.Delayed: return "Задержан";
                default: return status.ToString();
            }
        }

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

        private WorkType GetWorkTypeFromRussian(string russianName)
        {
            switch (russianName)
            {
                case "Фундамент": return WorkType.Foundation;
                case "Кладка": return WorkType.Brickwork;
                case "Сантехника": return WorkType.Plumbing;
                case "Электрика": return WorkType.Electrical;
                case "Отделка": return WorkType.Finishing;
                case "Дорожные работы": return WorkType.Roadwork;
                default: return WorkType.Foundation;
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

        private int GetSelectedScheduleId()
        {
            if (schedulesGrid.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Выберите график.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return -1;
            }
            return (int)schedulesGrid.CurrentRow.Cells["Id"].Value;
        }

        private void BtnAddSchedule_Click(object sender, EventArgs e)
        {
            var form = new AddWorkScheduleForm(repository);
            if (form.ShowDialog() == DialogResult.OK) LoadSchedules();
        }

        private void ContextMenu_Edit_Click(object? sender, EventArgs e)
        {
            int scheduleId = GetSelectedScheduleId();
            if (scheduleId < 0) return;
            var schedule = repository.GetWorkSchedule(scheduleId);
            if (schedule == null) return;
            var form = new AddWorkScheduleForm(repository, schedule);
            if (form.ShowDialog() == DialogResult.OK) LoadSchedules();
        }

        private void ContextMenu_ViewReports_Click(object? sender, EventArgs e)
        {
            int scheduleId = GetSelectedScheduleId();
            if (scheduleId < 0) return;
            var form = new ScheduleReportsForm(repository, scheduleId);
            form.ShowDialog();
        }

        private void ContextMenu_Delete_Click(object? sender, EventArgs e)
        {
            int scheduleId = GetSelectedScheduleId();
            if (scheduleId < 0) return;
            string workType = schedulesGrid.CurrentRow?.Cells["WorkType"].Value?.ToString() ?? "график";
            if (MessageBox.Show($"Удалить график работ \"{workType}\"?", "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                repository.DeleteWorkSchedule(scheduleId);
                LoadSchedules();
                MessageBox.Show("График удален.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    // ✅ ФОРМА ДОБАВЛЕНИЯ/РЕДАКТИРОВАНИЯ ГРАФИКА РАБОТ
    public class AddWorkScheduleForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly WorkSchedule editingSchedule;
        private ComboBox cmbObject, cmbWorkType, cmbBrigade;
        private DateTimePicker dtpPlannedStart, dtpPlannedEnd;
        private DateTimePicker dtpActualStart, dtpActualEnd;
        private CheckBox chkActualStart, chkActualEnd;

        private const int LABEL_WIDTH = 100;
        private const int FIELD_X = 120;
        private const int FIELD_WIDTH = 280;
        private const int ROW_HEIGHT = 35;
        private const int START_Y = 20;
        private const int CHECKBOX_WIDTH = 20;
        private const int DATETIME_WIDTH = 90;

        public AddWorkScheduleForm(IConstructionRepository repo) : this(repo, null) { }
        public AddWorkScheduleForm(IConstructionRepository repo, WorkSchedule scheduleToEdit)
        {
            repository = repo;
            editingSchedule = scheduleToEdit;
            InitializeForm();
            if (editingSchedule != null) FillFieldsForEdit();
        }

        private void InitializeForm()
        {
            this.Text = editingSchedule == null ? "Добавить график работ" : "Редактировать график работ";
            this.Size = new Size(450, 295);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = START_Y;

            var lblObject = new Label { Text = "Объект:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) };
            cmbObject = new ComboBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbObject.DataSource = repository.GetObjects();
            cmbObject.DisplayMember = "Name"; cmbObject.ValueMember = "Id";
            Controls.Add(lblObject); Controls.Add(cmbObject);
            yPos += ROW_HEIGHT;

            var lblWorkType = new Label { Text = "Вид работ:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) };
            cmbWorkType = new ComboBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbWorkType.Items.Add("Фундамент");
            cmbWorkType.Items.Add("Кладка");
            cmbWorkType.Items.Add("Сантехника");
            cmbWorkType.Items.Add("Электрика");
            cmbWorkType.Items.Add("Отделка");
            cmbWorkType.Items.Add("Дорожные работы");
            cmbWorkType.SelectedIndex = 0;
            Controls.Add(lblWorkType); Controls.Add(cmbWorkType);
            yPos += ROW_HEIGHT;

            var lblBrigade = new Label { Text = "Бригада:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) };
            cmbBrigade = new ComboBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBrigade.DataSource = repository.GetBrigades();
            cmbBrigade.DisplayMember = "Name"; cmbBrigade.ValueMember = "Id";
            Controls.Add(lblBrigade); Controls.Add(cmbBrigade);
            yPos += ROW_HEIGHT;

            var lblPlannedPeriod = new Label { Text = "Период работ:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 23), TextAlign = ContentAlignment.MiddleLeft };
            var lblFrom = new Label { Text = "с", Location = new Point(FIELD_X, yPos), Size = new Size(20, 23), TextAlign = ContentAlignment.MiddleLeft };
            dtpPlannedStart = new DateTimePicker { Location = new Point(FIELD_X + 25, yPos), Width = 110, Format = DateTimePickerFormat.Short };
            var lblTo = new Label { Text = "по", Location = new Point(FIELD_X + 140, yPos), Size = new Size(25, 23), TextAlign = ContentAlignment.MiddleCenter };
            dtpPlannedEnd = new DateTimePicker { Location = new Point(FIELD_X + 170, yPos), Width = 110, Format = DateTimePickerFormat.Short };
            Controls.Add(lblPlannedPeriod); Controls.Add(lblFrom); Controls.Add(dtpPlannedStart);
            Controls.Add(lblTo); Controls.Add(dtpPlannedEnd);
            yPos += ROW_HEIGHT;

            var lblActualPeriod = new Label { Text = "Факт. период:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 23), TextAlign = ContentAlignment.MiddleLeft };
            var lblActualFrom = new Label { Text = "с", Location = new Point(FIELD_X, yPos), Size = new Size(20, 23), TextAlign = ContentAlignment.MiddleLeft };
            chkActualStart = new CheckBox { Location = new Point(FIELD_X + 25, yPos), Size = new Size(CHECKBOX_WIDTH, 23), Checked = false, TabStop = false };
            dtpActualStart = new DateTimePicker { Location = new Point(FIELD_X + 25 + CHECKBOX_WIDTH, yPos), Width = DATETIME_WIDTH, Format = DateTimePickerFormat.Short, Enabled = false };
            var lblActualTo = new Label { Text = "по", Location = new Point(FIELD_X + 140, yPos), Size = new Size(25, 23), TextAlign = ContentAlignment.MiddleCenter };
            chkActualEnd = new CheckBox { Location = new Point(FIELD_X + 170, yPos), Size = new Size(CHECKBOX_WIDTH, 23), Checked = false, TabStop = false };
            dtpActualEnd = new DateTimePicker { Location = new Point(FIELD_X + 170 + CHECKBOX_WIDTH, yPos), Width = DATETIME_WIDTH, Format = DateTimePickerFormat.Short, Enabled = false };

            chkActualStart.CheckedChanged += (s, e) => dtpActualStart.Enabled = chkActualStart.Checked;
            chkActualEnd.CheckedChanged += (s, e) => dtpActualEnd.Enabled = chkActualEnd.Checked;

            Controls.Add(lblActualPeriod); Controls.Add(lblActualFrom);
            Controls.Add(chkActualStart); Controls.Add(dtpActualStart);
            Controls.Add(lblActualTo); Controls.Add(chkActualEnd); Controls.Add(dtpActualEnd);
            yPos += ROW_HEIGHT;

            var btnSave = new Button { Text = "Сохранить", Location = new Point(FIELD_X, yPos + 10), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(FIELD_X + 120, yPos + 10), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSave); Controls.Add(btnCancel);

            SetDefaultValues();
        }

        public void SetPreselectedObject(int objectId)
        {
            if (cmbObject.DataSource != null && cmbObject.Items.Count > 0)
            {
                cmbObject.SelectedValue = objectId;
            }
        }

        private void SetDefaultValues()
        {
            dtpPlannedStart.Value = DateTime.Today;
            dtpPlannedEnd.Value = DateTime.Today.AddMonths(1);
            if (editingSchedule == null)
            {
                chkActualStart.Checked = false; dtpActualStart.Enabled = false; dtpActualStart.Value = DateTime.Today;
                chkActualEnd.Checked = false; dtpActualEnd.Enabled = false; dtpActualEnd.Value = DateTime.Today.AddMonths(1);
            }
        }

        private void FillFieldsForEdit()
        {
            cmbObject.SelectedValue = editingSchedule.ObjectId;
            cmbWorkType.SelectedItem = GetWorkTypeInRussian(editingSchedule.WorkType);
            cmbBrigade.SelectedValue = editingSchedule.BrigadeId;
            dtpPlannedStart.Value = editingSchedule.PlannedStartDate;
            dtpPlannedEnd.Value = editingSchedule.PlannedEndDate;

            if (editingSchedule.ActualStartDate.HasValue) { chkActualStart.Checked = true; dtpActualStart.Enabled = true; dtpActualStart.Value = editingSchedule.ActualStartDate.Value; }
            else { chkActualStart.Checked = false; dtpActualStart.Enabled = false; dtpActualStart.Value = DateTime.Today; }

            if (editingSchedule.ActualEndDate.HasValue) { chkActualEnd.Checked = true; dtpActualEnd.Enabled = true; dtpActualEnd.Value = editingSchedule.ActualEndDate.Value; }
            else { chkActualEnd.Checked = false; dtpActualEnd.Enabled = false; dtpActualEnd.Value = DateTime.Today.AddMonths(1); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbObject.SelectedValue == null || cmbBrigade.SelectedValue == null || cmbWorkType.SelectedItem == null)
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            WorkType workType = GetWorkTypeFromRussian(cmbWorkType.SelectedItem.ToString());
            var schedule = editingSchedule ?? new WorkSchedule();
            schedule.ObjectId = (int)cmbObject.SelectedValue;
            schedule.WorkType = workType;
            schedule.BrigadeId = (int)cmbBrigade.SelectedValue;
            schedule.PlannedStartDate = dtpPlannedStart.Value;
            schedule.PlannedEndDate = dtpPlannedEnd.Value;
            schedule.ActualStartDate = chkActualStart.Checked ? dtpActualStart.Value : (DateTime?)null;
            schedule.ActualEndDate = chkActualEnd.Checked ? dtpActualEnd.Value : (DateTime?)null;

            if (editingSchedule == null) repository.AddWorkSchedule(schedule);
            else repository.UpdateWorkSchedule(editingSchedule);

            MessageBox.Show("График работ успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK; Close();
        }

        private WorkType GetWorkTypeFromRussian(string russianName)
        {
            switch (russianName)
            {
                case "Фундамент": return WorkType.Foundation;
                case "Кладка": return WorkType.Brickwork;
                case "Сантехника": return WorkType.Plumbing;
                case "Электрика": return WorkType.Electrical;
                case "Отделка": return WorkType.Finishing;
                case "Дорожные работы": return WorkType.Roadwork;
                default: return WorkType.Foundation;
            }
        }

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
    }

    // ✅ ФОРМА ПРОСМОТРА И ДОБАВЛЕНИЯ ОТЧЕТОВ ПО ГРАФИКУ РАБОТ
    public class ScheduleReportsForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int scheduleId;
        private DataGridView reportsGrid;
        private Button btnAddReport;

        public ScheduleReportsForm(IConstructionRepository repo, int scheduleId)
        {
            repository = repo;
            this.scheduleId = scheduleId;
            InitializeForm();
            LoadReports();
        }

        private void InitializeForm()
        {
            this.Text = "Отчеты по графику работ";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.WhiteSmoke };
            btnAddReport = new Button
            {
                Text = "Добавить отчет",
                Location = new Point(10, 5),
                Size = new Size(120, 30)
            };
            btnAddReport.Click += BtnAddReport_Click;
            topPanel.Controls.Add(btnAddReport);

            reportsGrid = new DataGridView
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

            reportsGrid.DataBindingComplete += (s, e) => AddRowNumbers(reportsGrid);
            SetupReportsGrid();

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Редактировать", null, (s, e) => EditReport());
            contextMenu.Items.Add("Удалить", null, (s, e) => DeleteReport());
            reportsGrid.ContextMenuStrip = contextMenu;
            reportsGrid.CellDoubleClick += (s, e) => EditReport();

            this.Controls.Add(reportsGrid);
            this.Controls.Add(topPanel);
        }

        private void SetupReportsGrid()
        {
            reportsGrid.Columns.Clear();

            reportsGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            // ✅ СКРЫТАЯ КОЛОНКА ID (для редактирования/удаления)
            reportsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            reportsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата", DataPropertyName = "ReportDate", FillWeight = 1 });
            reportsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Выполнено (%)", DataPropertyName = "Completed", FillWeight = 1 });
            // ✅ ДОП. РАСХОДЫ ПОСЛЕ "ВЫПОЛНЕНО"
            reportsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Доп. расходы", DataPropertyName = "AdditionalCosts", FillWeight = 1 });
            reportsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = "Description", FillWeight = 1 });
            reportsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Проблемы", DataPropertyName = "Issues", FillWeight = 1 });
        }

        private void LoadReports()
        {
            var reports = repository.GetWorkReports().Where(r => r.WorkScheduleId == scheduleId).Select(r => new
            {
                r.Id,
                ReportDate = r.ReportDate.ToString("dd.MM.yyyy"),
                Completed = $"{r.CompletedPercentage}%",
                AdditionalCosts = r.AdditionalCosts.HasValue ? $"{r.AdditionalCosts:C}" : "-",
                Description = r.Description ?? "-",
                Issues = r.Issues ?? "-"
            }).ToList();
            reportsGrid.DataSource = reports;
        }

        private void BtnAddReport_Click(object sender, EventArgs e)
        {
            var form = new AddWorkReportForm(repository, scheduleId);
            if (form.ShowDialog() == DialogResult.OK) LoadReports();
        }

        private void EditReport()
        {
            if (reportsGrid.CurrentRow?.DataBoundItem == null) return;
            var reportId = (int)reportsGrid.CurrentRow.Cells["Id"].Value;
            var report = repository.GetWorkReport(reportId);
            if (report == null) return;

            var form = new AddWorkReportForm(repository, scheduleId, report);
            if (form.ShowDialog() == DialogResult.OK) LoadReports();
        }

        private void DeleteReport()
        {
            if (reportsGrid.CurrentRow?.DataBoundItem == null) return;
            var reportId = (int)reportsGrid.CurrentRow.Cells["Id"].Value;
            var report = repository.GetWorkReport(reportId);
            if (report == null) return;

            if (MessageBox.Show($"Удалить отчет от {report.ReportDate:dd.MM.yyyy}?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                repository.DeleteWorkReport(reportId);
                LoadReports();
                MessageBox.Show("Отчет удален.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }

    // ✅ НОВАЯ ФОРМА: Добавление/редактирование отчета по графику работ
    public class AddWorkReportForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int scheduleId;
        private readonly WorkReport editingReport;
        private DateTimePicker dtpReportDate;
        private NumericUpDown nudCompleted;
        private TextBox txtAdditionalCosts, txtDescription, txtIssues;

        private const int LABEL_WIDTH = 120, FIELD_X = 130, FIELD_WIDTH = 280, ROW_HEIGHT = 35, START_Y = 20;

        public AddWorkReportForm(IConstructionRepository repo, int schedId) : this(repo, schedId, null) { }
        public AddWorkReportForm(IConstructionRepository repo, int schedId, WorkReport reportToEdit)
        {
            repository = repo;
            scheduleId = schedId;
            editingReport = reportToEdit;
            InitializeForm();
            if (editingReport != null) FillFieldsForEdit();
        }

        private void InitializeForm()
        {
            this.Text = editingReport == null ? "Добавить отчет" : "Редактировать отчет";
            this.Size = new Size(480, 295);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = START_Y;

            // Дата и Выполнено на одной строке
            Controls.Add(new Label { Text = "Дата:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            dtpReportDate = new DateTimePicker { Location = new Point(FIELD_X, yPos), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            Controls.Add(dtpReportDate);

            Controls.Add(new Label { Text = "Выполнено (%):", Location = new Point(270, yPos), Size = new Size(100, 20) });
            nudCompleted = new NumericUpDown { Location = new Point(380, yPos), Width = 60, Minimum = 0, Maximum = 100, Value = 0 };
            Controls.Add(nudCompleted);
            yPos += ROW_HEIGHT;

            // Доп. расходы
            Controls.Add(new Label { Text = "Доп. расходы:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            txtAdditionalCosts = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH };
            Controls.Add(txtAdditionalCosts);
            yPos += ROW_HEIGHT;

            // ✅ Описание (ОБЯЗАТЕЛЬНОЕ, минимум 3 символа)
            Controls.Add(new Label { Text = "Описание:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            txtDescription = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH, Multiline = true, Height = 40 };
            Controls.Add(txtDescription);
            yPos += 50;

            // Проблемы (НЕ обязательное)
            Controls.Add(new Label { Text = "Проблемы:", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
            txtIssues = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH, Multiline = true, Height = 40 };
            Controls.Add(txtIssues);
            yPos += 50;

            // Кнопки
            var btnSave = new Button { Text = "Сохранить", Location = new Point(FIELD_X, yPos + 10), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(FIELD_X + 120, yPos + 10), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSave); Controls.Add(btnCancel);
        }

        private void FillFieldsForEdit()
        {
            dtpReportDate.Value = editingReport.ReportDate;
            nudCompleted.Value = editingReport.CompletedPercentage;
            txtAdditionalCosts.Text = editingReport.AdditionalCosts?.ToString() ?? "";
            txtDescription.Text = editingReport.Description ?? "";
            txtIssues.Text = editingReport.Issues ?? "";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // ✅ ВАЛИДАЦИЯ: Описание обязательно, минимум 3 символа
            string description = txtDescription.Text.Trim();
            if (string.IsNullOrWhiteSpace(description) || description.Length < 3)
            {
                MessageBox.Show("Поле \"Описание\" обязательно и должно содержать не менее 3 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescription.Focus();
                return;
            }

            decimal? additionalCosts = null;
            if (!string.IsNullOrWhiteSpace(txtAdditionalCosts.Text) && decimal.TryParse(txtAdditionalCosts.Text, out decimal cost))
            {
                additionalCosts = cost;
            }

            var report = editingReport ?? new WorkReport();
            report.WorkScheduleId = scheduleId;
            report.ReportDate = dtpReportDate.Value;
            report.CompletedPercentage = (int)nudCompleted.Value;
            report.Description = description;  // ✅ Уже обрезано и проверено
            report.AdditionalCosts = additionalCosts;
            report.Issues = string.IsNullOrWhiteSpace(txtIssues.Text) ? null : txtIssues.Text.Trim();
            report.ReportedBy = 0;

            if (editingReport == null) repository.AddWorkReport(report);
            else repository.UpdateWorkReport(editingReport);

            MessageBox.Show("Отчет успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK; Close();
        }
    }
}