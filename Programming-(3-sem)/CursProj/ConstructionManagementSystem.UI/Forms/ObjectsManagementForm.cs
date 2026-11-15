using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public class ObjectsManagementForm : Form
    {
        private readonly IConstructionRepository repository;
        private DataGridView objectsGrid;
        private Button btnAddObject;
        private ContextMenuStrip contextMenuStrip;
        private ComboBox cmbFilterSite;
        private ComboBox cmbFilterType;

        // ✅ Хранение характеристик в памяти (временно)
        private Dictionary<int, ObjectSpecifications> specificationsCache = new Dictionary<int, ObjectSpecifications>();

        public ObjectsManagementForm(IConstructionRepository repo)
        {
            repository = repo;
            InitializeForm();
            LoadObjects();
        }

        private void InitializeForm()
        {
            this.Text = "Управление объектами";
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            objectsGrid = new DataGridView
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

            objectsGrid.DataBindingComplete += (s, e) => AddRowNumbers(objectsGrid);

            SetupObjectsGrid();

            contextMenuStrip = new ContextMenuStrip();

            var editMenuItem = new ToolStripMenuItem("Редактировать", null, ContextMenu_Edit_Click);
            var specsMenuItem = new ToolStripMenuItem("Характеристики", null, ContextMenu_ViewSpecifications_Click);
            var scheduleMenuItem = new ToolStripMenuItem("График работ", null, ContextMenu_ViewSchedule_Click);
            var estimateMenuItem = new ToolStripMenuItem("Смета", null, ContextMenu_ViewEstimate_Click);
            var reportMenuItem = new ToolStripMenuItem("Отчет", null, ContextMenu_GenerateReport_Click);
            var deleteMenuItem = new ToolStripMenuItem("Удалить", null, ContextMenu_Delete_Click);

            contextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                editMenuItem,
                specsMenuItem,
                scheduleMenuItem,
                estimateMenuItem,
                reportMenuItem,
                new ToolStripSeparator(),
                deleteMenuItem
            });

            objectsGrid.ContextMenuStrip = contextMenuStrip;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke
            };

            btnAddObject = new Button
            {
                Text = "Добавить объект",
                Location = new Point(10, 10),
                Size = new Size(160, 30)
            };
            btnAddObject.Click += BtnAddObject_Click;

            var separator = new Label
            {
                Location = new Point(181, 8),
                Size = new Size(2, 34),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightGray
            };

            cmbFilterSite = new ComboBox
            {
                Location = new Point(194, 12),
                Width = 182,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterSite.Items.Add("Все участки");
            cmbFilterSite.Items.AddRange(repository.GetSites().Select(s => s.Name).ToArray());
            cmbFilterSite.SelectedIndex = 0;
            cmbFilterSite.SelectedIndexChanged += (s, e) => LoadObjects();

            cmbFilterType = new ComboBox
            {
                Location = new Point(390, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterType.Items.Add("Все типы");
            cmbFilterType.Items.AddRange(GetAllObjectTypesInRussian());
            cmbFilterType.SelectedIndex = 0;
            cmbFilterType.SelectedIndexChanged += (s, e) => LoadObjects();

            topPanel.Controls.Add(btnAddObject);
            topPanel.Controls.Add(separator);
            topPanel.Controls.Add(cmbFilterSite);
            topPanel.Controls.Add(cmbFilterType);

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(objectsGrid);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);
        }

        private void SetupObjectsGrid()
        {
            objectsGrid.Columns.Clear();

            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Название",
                DataPropertyName = "Name",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Type",
                HeaderText = "Тип",
                DataPropertyName = "Type",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Site",
                HeaderText = "Участок",
                DataPropertyName = "SiteName",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Customer",
                HeaderText = "Заказчик",
                DataPropertyName = "CustomerName",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StartDate",
                HeaderText = "Начало",
                DataPropertyName = "StartDate",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PlannedEndDate",
                HeaderText = "Окончание",
                DataPropertyName = "PlannedEndDate",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Budget",
                HeaderText = "Бюджет",
                DataPropertyName = "Budget",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Статус",
                DataPropertyName = "Status",
                FillWeight = 1
            });
        }

        private void LoadObjects()
        {
            var ruCulture = new CultureInfo("ru-RU");

            var objects = repository.GetObjects().Select(o => new
            {
                o.Id,
                o.Name,
                Type = GetObjectTypeInRussian(o.Type),
                SiteName = repository.GetSite(o.SiteId)?.Name ?? "Неизвестно",
                CustomerName = repository.GetCustomer(o.CustomerId)?.Name ?? "Неизвестно",
                StartDate = o.StartDate.ToString("dd.MM.yyyy"),
                PlannedEndDate = o.PlannedEndDate.ToString("dd.MM.yyyy"),
                Budget = o.Budget.ToString("C", ruCulture),
                Status = GetObjectStatus(o)
            }).ToList();

            if (cmbFilterSite.SelectedIndex > 0)
            {
                var selectedSiteName = cmbFilterSite.SelectedItem.ToString();
                objects = objects.Where(o => o.SiteName == selectedSiteName).ToList();
            }

            if (cmbFilterType.SelectedIndex > 0)
            {
                var selectedType = cmbFilterType.SelectedItem.ToString();
                objects = objects.Where(o => o.Type == selectedType).ToList();
            }

            objectsGrid.DataSource = objects;
        }

        private string GetObjectStatus(ConstructionObject obj)
        {
            if (obj.ActualEndDate.HasValue)
                return "Завершен";

            var now = DateTime.Today;

            if (now < obj.StartDate.Date)
                return "Запланирован";

            if (now > obj.PlannedEndDate.Date)
                return "Завершен";

            return "В работе";
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

        private string[] GetAllObjectTypesInRussian()
        {
            return new string[]
            {
                "Жилой дом",
                "Больница",
                "Школа",
                "Мост",
                "Дорога"
            };
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
                default: return "Неизвестный тип";
            }
        }

        private int GetSelectedObjectId()
        {
            if (objectsGrid.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Выберите объект.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return -1;
            }

            return (int)objectsGrid.CurrentRow.Cells["Id"].Value;
        }

        private void BtnAddObject_Click(object sender, EventArgs e)
        {
            var form = new AddObjectForm(repository);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadObjects();
            }
        }

        private void ContextMenu_GenerateReport_Click(object? sender, EventArgs e)
        {
            if (objectsGrid.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Выберите объект для составления отчета.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int objectId = (int)objectsGrid.CurrentRow.Cells["Id"].Value;
            var form = new ObjectReportForm(repository, objectId, specificationsCache);
            form.ShowDialog();
        }

        private void ContextMenu_Edit_Click(object? sender, EventArgs e)
        {
            int objectId = GetSelectedObjectId();
            if (objectId < 0) return;

            var obj = repository.GetObject(objectId);
            if (obj == null)
            {
                MessageBox.Show("Не удалось загрузить данные объекта.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var form = new AddObjectForm(repository, obj);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadObjects();
            }
        }

        private void ContextMenu_ViewSpecifications_Click(object? sender, EventArgs e)
        {
            int objectId = GetSelectedObjectId();
            if (objectId < 0) return;

            var obj = repository.GetObject(objectId);
            if (obj == null)
            {
                MessageBox.Show("Не удалось загрузить данные объекта.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var form = new ObjectSpecificationsForm(repository, obj, specificationsCache);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadObjects();
            }
        }

        private void ContextMenu_Delete_Click(object? sender, EventArgs e)
        {
            int objectId = GetSelectedObjectId();
            if (objectId < 0) return;

            string objectName = objectsGrid.CurrentRow?.Cells["Name"].Value?.ToString() ?? "объект";

            var result = MessageBox.Show(
                $"Удалить объект \"{objectName}\" и все связанные с ним данные (графики, сметы, отчеты)?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                repository.DeleteObject(objectId);
                LoadObjects();
                MessageBox.Show("Объект удален.", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ContextMenu_ViewSchedule_Click(object? sender, EventArgs e)
        {
            int objectId = GetSelectedObjectId();
            if (objectId < 0) return;

            var form = new ObjectWorkScheduleForm(repository, objectId);
            form.ShowDialog();
        }

        private void ContextMenu_ViewEstimate_Click(object? sender, EventArgs e)
        {
            int objectId = GetSelectedObjectId();
            if (objectId < 0) return;

            var form = new ObjectEstimateForm(repository, objectId);
            form.ShowDialog();
        }
    }

    // Форма добавления/редактирования объекта
    public class AddObjectForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly ConstructionObject editingObject;
        private TextBox txtName;
        private ComboBox cmbType;
        private ComboBox cmbSite;
        private ComboBox cmbCustomer;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpPlannedEndDate;
        private TextBox txtBudget;

        private struct ObjectTypeItem
        {
            public string DisplayName { get; set; }
            public ObjectType Type { get; set; }

            public override string ToString()
            {
                return DisplayName;
            }
        }

        public AddObjectForm(IConstructionRepository repo) : this(repo, null) { }
        public AddObjectForm(IConstructionRepository repo, ConstructionObject objToEdit)
        {
            repository = repo;
            editingObject = objToEdit;
            InitializeForm();
            if (editingObject != null) FillFieldsForEdit();
        }

        private void InitializeForm()
        {
            this.Text = editingObject == null ? "Добавить объект" : "Редактировать объект";
            this.Size = new Size(450, 330);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;

            var lblName = new Label
            {
                Text = "* Название:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            txtName = new TextBox
            {
                Location = new Point(120, yPos),
                Width = 280
            };
            txtName.Leave += TxtName_Leave;
            yPos += 35;

            var lblType = new Label
            {
                Text = "Тип:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            cmbType = new ComboBox
            {
                Location = new Point(120, yPos),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var typeItems = new List<ObjectTypeItem>
            {
                new ObjectTypeItem { DisplayName = "Жилой дом", Type = ObjectType.ResidentialBuilding },
                new ObjectTypeItem { DisplayName = "Больница", Type = ObjectType.Hospital },
                new ObjectTypeItem { DisplayName = "Школа", Type = ObjectType.School },
                new ObjectTypeItem { DisplayName = "Мост", Type = ObjectType.Bridge },
                new ObjectTypeItem { DisplayName = "Дорога", Type = ObjectType.Road }
            };
            cmbType.DataSource = typeItems;
            cmbType.DisplayMember = "DisplayName";
            cmbType.ValueMember = "Type";

            yPos += 35;

            var lblSite = new Label
            {
                Text = "* Участок:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            cmbSite = new ComboBox
            {
                Location = new Point(120, yPos),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSite.DataSource = repository.GetSites();
            cmbSite.DisplayMember = "Name";
            cmbSite.ValueMember = "Id";
            yPos += 35;

            var lblCustomer = new Label
            {
                Text = "* Заказчик:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            cmbCustomer = new ComboBox
            {
                Location = new Point(120, yPos),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCustomer.DataSource = repository.GetCustomers();
            cmbCustomer.DisplayMember = "Name";
            cmbCustomer.ValueMember = "Id";
            yPos += 35;

            var lblBudget = new Label
            {
                Text = "* Бюджет:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            txtBudget = new TextBox
            {
                Location = new Point(120, yPos),
                Width = 280
            };

            txtBudget.TextChanged += TxtBudget_TextChanged;
            txtBudget.Leave += TxtBudget_Leave;

            yPos += 35;

            var lblPeriod = new Label
            {
                Text = "Период работ:",
                Location = new Point(10, yPos),
                Size = new Size(100, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblFrom = new Label
            {
                Text = "с",
                Location = new Point(120, yPos),
                Size = new Size(20, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };

            dtpStartDate = new DateTimePicker
            {
                Location = new Point(145, yPos),
                Width = 110,
                Value = DateTime.Now,
                Format = DateTimePickerFormat.Short
            };

            var lblTo = new Label
            {
                Text = "по",
                Location = new Point(260, yPos),
                Size = new Size(25, 23),
                TextAlign = ContentAlignment.MiddleCenter
            };

            dtpPlannedEndDate = new DateTimePicker
            {
                Location = new Point(290, yPos),
                Width = 110,
                Value = DateTime.Now.AddMonths(6),
                Format = DateTimePickerFormat.Short
            };
            yPos += 35;

            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(120, yPos + 10),
                Size = new Size(100, 30)
            };
            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(240, yPos + 10),
                Size = new Size(100, 30)
            };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[]
            {
                lblName, txtName,
                lblType, cmbType,
                lblSite, cmbSite,
                lblCustomer, cmbCustomer,
                lblBudget, txtBudget,
                lblPeriod, lblFrom, dtpStartDate, lblTo, dtpPlannedEndDate,
                btnSave, btnCancel
            });
        }

        private void TxtName_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtName.Text))
            {
                txtName.Text = char.ToUpper(txtName.Text[0]) + txtName.Text.Substring(1).ToLower();
                txtName.SelectionStart = txtName.Text.Length;
            }
        }

        private void TxtBudget_TextChanged(object sender, EventArgs e)
        {
            if (txtBudget.Text.Contains(","))
            {
                string text = txtBudget.Text.Replace(",", ".");
                txtBudget.Text = text;
                txtBudget.SelectionStart = text.Length;
            }
        }

        private void TxtBudget_Leave(object sender, EventArgs e)
        {
            string budgetText = txtBudget.Text.Replace("₽", "").Replace(" ", "").Trim();

            if (decimal.TryParse(budgetText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
            {
                if (value < 0)
                {
                    MessageBox.Show("Бюджет не может быть отрицательным!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtBudget.Focus();
                    return;
                }

                if (value < 1000)
                {
                    MessageBox.Show("Бюджет не может быть меньше 1000 ₽!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtBudget.Focus();
                    return;
                }

                txtBudget.Text = value.ToString("F2") + " ₽";
            }
        }

        private void FillFieldsForEdit()
        {
            txtName.Text = editingObject.Name;

            if (cmbType.DataSource is List<ObjectTypeItem> items)
            {
                var selectedItem = items.FirstOrDefault(i => i.Type == editingObject.Type);
                cmbType.SelectedItem = selectedItem;
            }

            cmbSite.SelectedValue = editingObject.SiteId;
            cmbCustomer.SelectedValue = editingObject.CustomerId;

            txtBudget.Text = editingObject.Budget.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',') + " ₽";

            dtpStartDate.Value = editingObject.StartDate;
            dtpPlannedEndDate.Value = editingObject.PlannedEndDate;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (name.Length < 2)
            {
                MessageBox.Show("Название объекта должно содержать не менее 2 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            string budgetText = txtBudget.Text.Replace("₽", "").Replace(" ", "").Trim();

            if (!decimal.TryParse(budgetText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal budget))
            {
                MessageBox.Show("Введите корректный бюджет (например: 1000.50 или 1000,50)!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtBudget.Focus();
                return;
            }

            if (budget < 0)
            {
                MessageBox.Show("Бюджет не может быть отрицательным!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtBudget.Focus();
                return;
            }

            if (budget < 1000)
            {
                MessageBox.Show("Бюджет не может быть меньше 1000 ₽!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBudget.Focus();
                return;
            }

            if (cmbSite.SelectedValue == null)
            {
                MessageBox.Show("Выберите участок!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbSite.Focus();
                return;
            }

            var obj = editingObject ?? new ConstructionObject();
            obj.Name = name;

            if (cmbType.SelectedItem is ObjectTypeItem selectedType)
                obj.Type = selectedType.Type;

            obj.SiteId = (int)cmbSite.SelectedValue;
            obj.CustomerId = (int)cmbCustomer.SelectedValue;
            obj.Budget = budget;
            obj.StartDate = dtpStartDate.Value;
            obj.PlannedEndDate = dtpPlannedEndDate.Value;

            if (editingObject == null)
                repository.AddObject(obj);
            else
                repository.UpdateObject(editingObject);

            MessageBox.Show("Объект успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // ✅ КЛАСС ХАРАКТЕРИСТИК (должен быть в файле!)
    public class ObjectSpecifications
    {
        public int ObjectId { get; set; }
        public string Spec1 { get; set; }
        public string Spec2 { get; set; }
        public string Spec3 { get; set; }
        public string Spec4 { get; set; }
    }

    // Форма характеристик объекта
    public class ObjectSpecificationsForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly ConstructionObject objectItem;
        private readonly Dictionary<int, ObjectSpecifications> specificationsCache;

        private TextBox txtSpec1, txtSpec2, txtSpec3;
        private TextBox txtSpec4;
        private ComboBox cmbSpec4;
        private ObjectType objectType;

        private const int LABEL_WIDTH = 150;
        private const int FIELD_X = 160;
        private const int FIELD_WIDTH = 240;
        private const int ROW_HEIGHT = 35;
        private const int START_Y = 20;

        public ObjectSpecificationsForm(IConstructionRepository repo, ConstructionObject obj, Dictionary<int, ObjectSpecifications> cache)
        {
            repository = repo;
            objectItem = obj;
            specificationsCache = cache;
            objectType = obj.Type;
            InitializeForm();
            FillFieldsForEdit();
        }

        private void InitializeForm()
        {
            this.Text = $"Характеристики: {objectItem.Name}";
            this.Size = new Size(450, 260);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = START_Y;

            var fieldLabels = GetSpecificationLabels(objectType);
            bool isRoad = objectType == ObjectType.Road;

            var lblSpec1 = new Label { Text = fieldLabels[0] + ":", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) };
            txtSpec1 = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH };
            Controls.Add(lblSpec1); Controls.Add(txtSpec1);
            yPos += ROW_HEIGHT;

            var lblSpec2 = new Label { Text = fieldLabels[1] + ":", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) };
            txtSpec2 = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH };
            Controls.Add(lblSpec2); Controls.Add(txtSpec2);
            yPos += ROW_HEIGHT;

            var lblSpec3 = new Label { Text = fieldLabels[2] + ":", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) };
            txtSpec3 = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH };
            Controls.Add(lblSpec3); Controls.Add(txtSpec3);
            yPos += ROW_HEIGHT;

            var lblSpec4 = new Label { Text = fieldLabels[3] + ":", Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) };

            if (isRoad)
            {
                cmbSpec4 = new ComboBox
                {
                    Location = new Point(FIELD_X, yPos),
                    Width = FIELD_WIDTH,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbSpec4.Items.AddRange(new[] { "Асфальт", "Бетон", "Гравий", "Грунт", "Плиты" });
                Controls.Add(lblSpec4); Controls.Add(cmbSpec4);
            }
            else
            {
                txtSpec4 = new TextBox { Location = new Point(FIELD_X, yPos), Width = FIELD_WIDTH };
                Controls.Add(lblSpec4); Controls.Add(txtSpec4);
            }
            yPos += ROW_HEIGHT + 10;

            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(FIELD_X, yPos),
                Size = new Size(100, 30)
            };
            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(FIELD_X + 120, yPos),
                Size = new Size(100, 30)
            };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnSave); Controls.Add(btnCancel);
        }

        private string[] GetSpecificationLabels(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.ResidentialBuilding:
                    return new[] { "Этажность", "Кол-во квартир", "Общая площадь (м²)", "Жилая площадь (м²)" };
                case ObjectType.Hospital:
                    return new[] { "Кол-во коек", "Кол-во отделений", "Общая площадь (м²)", "Кол-во операционных" };
                case ObjectType.School:
                    return new[] { "Кол-во классов", "Вместимость (учеников)", "Общая площадь (м²)", "Кол-во этажей" };
                case ObjectType.Bridge:
                    return new[] { "Длина (м)", "Ширина (м)", "Грузоподъёмность (тонн)", "Кол-во пролётов" };
                case ObjectType.Road:
                    return new[] { "Длина (км)", "Ширина (м)", "Кол-во полос", "Тип покрытия" };
                default:
                    return new[] { "Параметр 1", "Параметр 2", "Параметр 3", "Параметр 4" };
            }
        }

        private void FillFieldsForEdit()
        {
            if (specificationsCache.ContainsKey(objectItem.Id))
            {
                var specs = specificationsCache[objectItem.Id];
                txtSpec1.Text = specs.Spec1 ?? "";
                txtSpec2.Text = specs.Spec2 ?? "";
                txtSpec3.Text = specs.Spec3 ?? "";

                if (objectType == ObjectType.Road)
                {
                    if (cmbSpec4 != null && cmbSpec4.Items.Contains(specs.Spec4))
                        cmbSpec4.SelectedItem = specs.Spec4;
                    else if (cmbSpec4 != null)
                        cmbSpec4.SelectedIndex = 0;
                }
                else
                {
                    if (txtSpec4 != null)
                        txtSpec4.Text = specs.Spec4 ?? "";
                }
            }
            else
            {
                if (objectType == ObjectType.Road && cmbSpec4 != null)
                    cmbSpec4.SelectedIndex = 0;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string spec4Value = objectType == ObjectType.Road
                ? (cmbSpec4?.SelectedItem?.ToString() ?? "")
                : (txtSpec4?.Text.Trim() ?? "");

            var specs = new ObjectSpecifications
            {
                ObjectId = objectItem.Id,
                Spec1 = txtSpec1.Text.Trim(),
                Spec2 = txtSpec2.Text.Trim(),
                Spec3 = txtSpec3.Text.Trim(),
                Spec4 = spec4Value
            };

            if (specificationsCache.ContainsKey(objectItem.Id))
            {
                specificationsCache[objectItem.Id] = specs;
            }
            else
            {
                specificationsCache.Add(objectItem.Id, specs);
            }

            MessageBox.Show("Характеристики успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // Форма просмотра графика работ объекта
    public class ObjectWorkScheduleForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int objectId;
        private DataGridView scheduleGrid;

        public ObjectWorkScheduleForm(IConstructionRepository repo, int objId)
        {
            repository = repo;
            objectId = objId;
            InitializeForm();
            LoadSchedule();
        }

        private void InitializeForm()
        {
            var obj = repository.GetObject(objectId);
            this.Text = $"График работ: {obj?.Name ?? "Неизвестно"}";
            this.Size = new Size(900, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            scheduleGrid = new DataGridView
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

            scheduleGrid.DataBindingComplete += (s, e) => AddRowNumbers(scheduleGrid);

            SetupScheduleGrid();
            this.Controls.Add(scheduleGrid);
        }

        private void SetupScheduleGrid()
        {
            scheduleGrid.Columns.Clear();

            scheduleGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            scheduleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Вид работ", DataPropertyName = "WorkType", FillWeight = 1 });
            scheduleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Бригада", DataPropertyName = "BrigadeName", FillWeight = 1 });
            scheduleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Начало", DataPropertyName = "PlannedStart", FillWeight = 1 });
            scheduleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Окончание", DataPropertyName = "PlannedEnd", FillWeight = 1 });
            scheduleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Факт. начало", DataPropertyName = "ActualStart", FillWeight = 1 });
            scheduleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Факт. окончание", DataPropertyName = "ActualEnd", FillWeight = 1 });
            scheduleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = "Status", FillWeight = 1 });
        }

        private void LoadSchedule()
        {
            var schedules = repository.GetWorkSchedulesByObject(objectId).Select(s => new
            {
                WorkType = GetWorkTypeInRussian(s.WorkType),
                BrigadeName = repository.GetBrigade(s.BrigadeId)?.Name ?? "Неизвестно",
                PlannedStart = s.PlannedStartDate.ToString("dd.MM.yyyy"),
                PlannedEnd = s.PlannedEndDate.ToString("dd.MM.yyyy"),
                ActualStart = s.ActualStartDate?.ToString("dd.MM.yyyy") ?? "-",
                ActualEnd = s.ActualEndDate?.ToString("dd.MM.yyyy") ?? "-",
                Status = GetStatusInRussian(CalculateStatus(s))
            }).ToList();

            scheduleGrid.DataSource = schedules;
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

    // Форма просмотра сметы объекта
    public class ObjectEstimateForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int objectId;
        private DataGridView estimateGrid;
        private Label lblTotalBudget;
        private Label lblTotalActual;

        public ObjectEstimateForm(IConstructionRepository repo, int objId)
        {
            repository = repo;
            objectId = objId;
            InitializeForm();
            LoadEstimate();
        }

        private void InitializeForm()
        {
            var obj = repository.GetObject(objectId);
            this.Text = $"Смета: {obj?.Name ?? "Неизвестно"}";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            estimateGrid = new DataGridView
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

            estimateGrid.DataBindingComplete += (s, e) => AddRowNumbers(estimateGrid);

            SetupEstimateGrid();

            var summaryPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.WhiteSmoke, Padding = new Padding(10) };
            lblTotalBudget = new Label { Text = "Плановая стоимость: ", Location = new Point(10, 10), AutoSize = true };
            lblTotalActual = new Label { Text = "Фактическая стоимость: ", Location = new Point(10, 30), AutoSize = true };
            summaryPanel.Controls.Add(lblTotalBudget);
            summaryPanel.Controls.Add(lblTotalActual);

            this.Controls.Add(estimateGrid);
            this.Controls.Add(summaryPanel);
        }

        private void SetupEstimateGrid()
        {
            estimateGrid.Columns.Clear();

            estimateGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            estimateGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Материал", DataPropertyName = "Material", FillWeight = 1 });
            estimateGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ед. изм.", DataPropertyName = "Unit", FillWeight = 1 });
            estimateGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "План. кол-во", DataPropertyName = "PlannedQty", FillWeight = 1 });
            estimateGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Факт. кол-во", DataPropertyName = "ActualQty", FillWeight = 1 });
            estimateGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Цена за ед.", DataPropertyName = "UnitPrice", FillWeight = 1 });
            estimateGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "План. стоимость", DataPropertyName = "TotalCost", FillWeight = 1 });
            estimateGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Факт. стоимость", DataPropertyName = "ActualCost", FillWeight = 1 });
            estimateGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Перерасход", DataPropertyName = "Overrun", FillWeight = 1 });
        }

        private void LoadEstimate()
        {
            var ruCulture = new CultureInfo("ru-RU");

            var estimates = repository.GetEstimatesByObject(objectId).Select(e => new
            {
                e.MaterialName,
                e.Unit,
                PlannedQty = e.PlannedQuantity.ToString("F2"),
                ActualQty = e.ActualQuantity.ToString("F2"),
                UnitPrice = e.UnitPrice.ToString("C", ruCulture),
                TotalCost = e.TotalCost.ToString("C", ruCulture),
                ActualCost = e.ActualCost.ToString("C", ruCulture),
                Overrun = e.ActualQuantity > e.PlannedQuantity
                    ? (e.ActualCost - e.TotalCost).ToString("C", ruCulture)
                    : "-"
            }).ToList();

            estimateGrid.DataSource = estimates;

            var totalBudget = estimates.Sum(e =>
                decimal.Parse(e.TotalCost.Replace("₽", "").Replace(" ", "").Trim(), ruCulture));

            var totalActual = estimates.Sum(e =>
                decimal.Parse(e.ActualCost.Replace("₽", "").Replace(" ", "").Trim(), ruCulture));

            lblTotalBudget.Text = $"Плановая стоимость: {totalBudget.ToString("C", ruCulture)}";
            lblTotalActual.Text = $"Фактическая стоимость: {totalActual.ToString("C", ruCulture)}";

            if (totalActual > totalBudget)
                lblTotalActual.ForeColor = Color.Red;
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

    // ✅ Форма просмотра отчета объекта (с характеристиками)
    public class ObjectReportForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int objectId;
        private readonly Dictionary<int, ObjectSpecifications> specificationsCache;
        private TextBox txtReport;

        public ObjectReportForm(IConstructionRepository repo, int objId, Dictionary<int, ObjectSpecifications> cache)
        {
            repository = repo;
            objectId = objId;
            specificationsCache = cache;
            InitializeForm();
            LoadReport();
        }

        private void InitializeForm()
        {
            var obj = repository.GetObject(objectId);
            this.Text = $"Отчет: {obj?.Name ?? "Неизвестно"}";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            txtReport = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Courier New", 10),
                ScrollBars = ScrollBars.Both
            };

            this.Controls.Add(txtReport);
        }

        private void LoadReport()
        {
            var obj = repository.GetObject(objectId);
            if (obj == null) return;

            var schedules = repository.GetWorkSchedulesByObject(objectId);
            var estimates = repository.GetEstimatesByObject(objectId);
            var workReports = repository.GetWorkReports()
                .Where(r => schedules.Any(s => s.Id == r.WorkScheduleId))
                .ToList();

            var reportText = new System.Text.StringBuilder();
            reportText.AppendLine($"ОТЧЕТ ПО СТРОИТЕЛЬСТВУ ОБЪЕКТА");
            reportText.AppendLine($"================================");
            reportText.AppendLine();
            reportText.AppendLine($"Название: {obj.Name}");
            reportText.AppendLine($"Тип: {GetObjectTypeInRussian(obj.Type)}");
            reportText.AppendLine($"Заказчик: {repository.GetCustomer(obj.CustomerId)?.Name ?? "Неизвестно"}");
            reportText.AppendLine($"Дата начала: {obj.StartDate:dd.MM.yyyy}");
            reportText.AppendLine($"Плановая дата окончания: {obj.PlannedEndDate:dd.MM.yyyy}");
            reportText.AppendLine($"Фактическая дата окончания: {(obj.ActualEndDate.HasValue ? obj.ActualEndDate.Value.ToString("dd.MM.yyyy") : "Не завершен")}");

            var ruCulture = new CultureInfo("ru-RU");
            reportText.AppendLine($"Плановый бюджет: {obj.Budget.ToString("C", ruCulture)}");

            // ✅ РАСЧЕТ ФАКТИЧЕСКИХ ЗАТРАТ из сметы
            decimal actualCost = estimates.Sum(e => e.ActualCost);
            reportText.AppendLine($"Фактические затраты: {actualCost.ToString("C", ruCulture)}");

            // ✅ Характеристики объекта
            if (specificationsCache.ContainsKey(objectId))
            {
                var specs = specificationsCache[objectId];
                var labels = GetSpecificationLabels(obj.Type);

                reportText.AppendLine();
                reportText.AppendLine($"ХАРАКТЕРИСТИКИ:");
                reportText.AppendLine($"--------------------------------");

                if (!string.IsNullOrWhiteSpace(specs.Spec1))
                    reportText.AppendLine($"{labels[0]}: {specs.Spec1}");
                if (!string.IsNullOrWhiteSpace(specs.Spec2))
                    reportText.AppendLine($"{labels[1]}: {specs.Spec2}");
                if (!string.IsNullOrWhiteSpace(specs.Spec3))
                    reportText.AppendLine($"{labels[2]}: {specs.Spec3}");
                if (!string.IsNullOrWhiteSpace(specs.Spec4))
                    reportText.AppendLine($"{labels[3]}: {specs.Spec4}");
            }

            reportText.AppendLine();
            reportText.AppendLine($"ГРАФИК РАБОТ:");
            reportText.AppendLine($"--------------------------------");
            foreach (var schedule in schedules)
            {
                reportText.AppendLine($"{GetWorkTypeInRussian(schedule.WorkType)}: {schedule.PlannedStartDate:dd.MM.yyyy} - {schedule.PlannedEndDate:dd.MM.yyyy} | Статус: {GetStatusInRussian(CalculateStatus(schedule))}");
            }

            reportText.AppendLine();
            reportText.AppendLine($"СМЕТА:");
            reportText.AppendLine($"--------------------------------");
            foreach (var estimate in estimates)
            {
                reportText.AppendLine($"{estimate.MaterialName}: {estimate.PlannedQuantity} {estimate.Unit} x {estimate.UnitPrice.ToString("C", ruCulture)} = {estimate.TotalCost.ToString("C", ruCulture)}");
            }

            reportText.AppendLine();
            reportText.AppendLine($"ОТЧЕТЫ ПО ВЫПОЛНЕНИЮ:");
            reportText.AppendLine($"--------------------------------");

            if (workReports.Any())
            {
                foreach (var report in workReports)
                {
                    reportText.AppendLine($"{report.ReportDate:dd.MM.yyyy}: {report.Description} | Выполнено: {report.CompletedPercentage}%");
                    if (!string.IsNullOrEmpty(report.Issues))
                    {
                        reportText.AppendLine($"  Проблемы: {report.Issues}");
                    }
                }
            }
            else
            {
                reportText.AppendLine("Отчеты по выполнению отсутствуют");
            }

            txtReport.Text = reportText.ToString();
        }

        private string[] GetSpecificationLabels(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.ResidentialBuilding:
                    return new[] { "Этажность", "Кол-во квартир", "Общая площадь (м²)", "Жилая площадь (м²)" };
                case ObjectType.Hospital:
                    return new[] { "Кол-во коек", "Кол-во отделений", "Общая площадь (м²)", "Кол-во операционных" };
                case ObjectType.School:
                    return new[] { "Кол-во классов", "Вместимость (учеников)", "Общая площадь (м²)", "Кол-во этажей" };
                case ObjectType.Bridge:
                    return new[] { "Длина (м)", "Ширина (м)", "Грузоподъёмность (тонн)", "Кол-во пролётов" };
                case ObjectType.Road:
                    return new[] { "Длина (км)", "Ширина (м)", "Кол-во полос", "Тип покрытия" };
                default:
                    return new[] { "Параметр 1", "Параметр 2", "Параметр 3", "Параметр 4" };
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
    }
}