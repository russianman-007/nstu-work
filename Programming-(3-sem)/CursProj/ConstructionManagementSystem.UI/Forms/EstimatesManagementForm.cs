using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Globalization;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public class EstimatesManagementForm : Form
    {
        private readonly IConstructionRepository repository;
        private DataGridView estimatesGrid;
        private Button btnAddEstimate;
        private ContextMenuStrip contextMenuStrip;
        private ComboBox cmbFilterObject;

        public EstimatesManagementForm(IConstructionRepository repo)
        {
            repository = repo;
            InitializeForm();
            LoadEstimates();
        }

        private void InitializeForm()
        {
            this.Text = "Управление сметами";
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            estimatesGrid = new DataGridView
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

            estimatesGrid.DataBindingComplete += (s, e) => AddRowNumbers(estimatesGrid);

            SetupEstimatesGrid();

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Редактировать", null, ContextMenu_Edit_Click);
            contextMenuStrip.Items.Add("Перерасходы", null, ContextMenu_ViewOverruns_Click);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add("Удалить", null, ContextMenu_Delete_Click);

            estimatesGrid.ContextMenuStrip = contextMenuStrip;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke
            };

            btnAddEstimate = new Button
            {
                Text = "Добавить смету",
                Location = new Point(10, 10),
                Size = new Size(160, 30)
            };
            btnAddEstimate.Click += BtnAddEstimate_Click;

            var separator = new Label
            {
                Location = new Point(181, 8),
                Size = new Size(2, 34),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightGray
            };

            // ✅ ИСПРАВЛЕНО: Фильтр по объектам с названиями
            cmbFilterObject = new ComboBox
            {
                Location = new Point(194, 12),
                Width = 205,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",  // ✅ Показывать название объекта
                ValueMember = "Id"       // ✅ Хранить ID объекта
            };
            cmbFilterObject.Items.Add(new ConstructionObject { Id = 0, Name = "Все объекты" });
            cmbFilterObject.Items.AddRange(repository.GetObjects().ToArray());
            cmbFilterObject.SelectedIndex = 0;
            cmbFilterObject.SelectedIndexChanged += (s, e) => LoadEstimates();

            topPanel.Controls.Add(btnAddEstimate);
            topPanel.Controls.Add(separator);
            topPanel.Controls.Add(cmbFilterObject);

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(estimatesGrid);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);
        }

        private void SetupEstimatesGrid()
        {
            estimatesGrid.Columns.Clear();

            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Object",
                HeaderText = "Объект",
                DataPropertyName = "ObjectName",
                FillWeight = 1
            });
            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Material",
                HeaderText = "Материал",
                DataPropertyName = "MaterialName",
                FillWeight = 1
            });
            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Unit",
                HeaderText = "Ед. изм.",
                DataPropertyName = "Unit",
                FillWeight = 1
            });
            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PlannedQty",
                HeaderText = "План. кол-во",
                DataPropertyName = "PlannedQty",
                FillWeight = 1
            });
            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ActualQty",
                HeaderText = "Факт. кол-во",
                DataPropertyName = "ActualQty",
                FillWeight = 1
            });
            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UnitPrice",
                HeaderText = "Цена за ед.",
                DataPropertyName = "UnitPrice",
                FillWeight = 1
            });
            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalCost",
                HeaderText = "План. стоимость",
                DataPropertyName = "TotalCost",
                FillWeight = 1
            });
            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ActualCost",
                HeaderText = "Факт. стоимость",
                DataPropertyName = "ActualCost",
                FillWeight = 1
            });
            estimatesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Overrun",
                HeaderText = "Перерасход",
                DataPropertyName = "Overrun",
                FillWeight = 1
            });
        }

        private void LoadEstimates()
        {
            var ruCulture = new CultureInfo("ru-RU");

            var estimates = repository.GetEstimates().Select(e => new
            {
                e.Id,
                ObjectName = repository.GetObject(e.ObjectId)?.Name ?? "Неизвестно",
                e.MaterialName,
                e.Unit,
                PlannedQty = e.PlannedQuantity.ToString("F2"),
                ActualQty = e.ActualQuantity.ToString("F2"),
                UnitPrice = e.UnitPrice.ToString("C", ruCulture),
                TotalCost = e.TotalCost.ToString("C", ruCulture),
                ActualCost = e.ActualCost.ToString("C", ruCulture),
                Overrun = e.ActualQuantity > e.PlannedQuantity ?
                    (e.ActualCost - e.TotalCost).ToString("C", ruCulture) : "-"
            }).ToList();

            // ✅ ИСПРАВЛЕНО: Фильтр по объектам через ID
            if (cmbFilterObject.SelectedIndex > 0 && cmbFilterObject.SelectedItem is ConstructionObject selectedObject)
            {
                if (selectedObject.Id > 0)  // Не "Все объекты"
                {
                    estimates = estimates.Where(e =>
                        repository.GetEstimates().Any(est => est.ObjectId == selectedObject.Id && est.Id.ToString() == e.Id.ToString())
                    ).ToList();
                }
            }

            estimatesGrid.DataSource = estimates;
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

        private int GetSelectedEstimateId()
        {
            if (estimatesGrid.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Выберите смету.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return -1;
            }
            return (int)estimatesGrid.CurrentRow.Cells["Id"].Value;
        }

        private void BtnAddEstimate_Click(object sender, EventArgs e)
        {
            var form = new AddEstimateForm(repository);
            if (form.ShowDialog() == DialogResult.OK) LoadEstimates();
        }

        private void ContextMenu_Edit_Click(object? sender, EventArgs e)
        {
            int estimateId = GetSelectedEstimateId();
            if (estimateId < 0) return;
            var estimate = repository.GetEstimate(estimateId);
            if (estimate == null) return;
            var form = new AddEstimateForm(repository, estimate);
            if (form.ShowDialog() == DialogResult.OK) LoadEstimates();
        }

        private void ContextMenu_ViewOverruns_Click(object? sender, EventArgs e)
        {
            var ruCulture = new CultureInfo("ru-RU");

            var overruns = repository.GetEstimatesWithOverrun().Select(e => new
            {
                ObjectName = repository.GetObject(e.ObjectId)?.Name ?? "Неизвестно",
                e.MaterialName,
                PlannedQty = e.PlannedQuantity.ToString("F2"),
                ActualQty = e.ActualQuantity.ToString("F2"),
                OverrunQty = (e.ActualQuantity - e.PlannedQuantity).ToString("F2"),
                OverrunCost = (e.ActualCost - e.TotalCost).ToString("C", ruCulture)
            }).ToList();
            var reportForm = new OverrunReportForm(overruns);
            reportForm.ShowDialog();
        }

        private void ContextMenu_Delete_Click(object? sender, EventArgs e)
        {
            int estimateId = GetSelectedEstimateId();
            if (estimateId < 0) return;
            string materialName = estimatesGrid.CurrentRow?.Cells["Material"].Value?.ToString() ?? "смета";
            if (MessageBox.Show($"Удалить смету на материал \"{materialName}\"?", "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                repository.DeleteEstimate(estimateId);
                LoadEstimates();
                MessageBox.Show("Смета удалена.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    // Форма добавления/редактирования сметы
    public class AddEstimateForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly Estimate editingEstimate;
        private ComboBox cmbObject, cmbUnit;
        private TextBox txtMaterialName, txtPlannedQuantity, txtActualQuantity, txtUnitPrice;

        private const int LABEL_WIDTH = 150, FIELD_X = 160, FIELD_WIDTH = 240, ROW_HEIGHT = 35, START_Y = 20;

        public AddEstimateForm(IConstructionRepository repo) : this(repo, null) { }
        public AddEstimateForm(IConstructionRepository repo, Estimate estimateToEdit)
        {
            repository = repo;
            editingEstimate = estimateToEdit;
            InitializeForm();
            if (editingEstimate != null) FillFieldsForEdit();
        }

        private void InitializeForm()
        {
            this.Text = editingEstimate == null ? "Добавить смету" : "Редактировать смету";
            this.Size = new Size(450, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = START_Y;
            void AddField(string label, Control control)
            {
                Controls.Add(new Label { Text = label, Location = new Point(10, yPos), Size = new Size(LABEL_WIDTH, 20) });
                Controls.Add(control);
                control.Location = new Point(FIELD_X, yPos);
                if (control is ComboBox cb) cb.Width = FIELD_WIDTH;
                else if (control is TextBox tb) tb.Width = FIELD_WIDTH;
                yPos += ROW_HEIGHT;
            }

            cmbObject = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbObject.DataSource = repository.GetObjects();
            cmbObject.DisplayMember = "Name"; cmbObject.ValueMember = "Id";
            AddField("Объект:", cmbObject);

            txtMaterialName = new TextBox();
            AddField("Материал:", txtMaterialName);

            cmbUnit = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUnit.Items.AddRange(new object[] { "м³", "м²", "шт", "кг", "т", "л" });
            cmbUnit.SelectedIndex = 0;
            AddField("Ед. изм.:", cmbUnit);

            txtPlannedQuantity = new TextBox();
            AddField("План. кол-во:", txtPlannedQuantity);

            txtActualQuantity = new TextBox { Text = "0" };
            AddField("Факт. кол-во:", txtActualQuantity);

            txtUnitPrice = new TextBox();
            AddField("Цена за ед.:", txtUnitPrice);

            txtUnitPrice.TextChanged += TxtUnitPrice_TextChanged;
            txtUnitPrice.Leave += TxtUnitPrice_Leave;

            yPos += 10;

            var btnSave = new Button { Text = "Сохранить", Location = new Point(FIELD_X, yPos), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(FIELD_X + 120, yPos), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSave); Controls.Add(btnCancel);
        }

        private void TxtUnitPrice_TextChanged(object sender, EventArgs e)
        {
            if (txtUnitPrice.Text.Contains(","))
            {
                string text = txtUnitPrice.Text.Replace(",", ".");
                txtUnitPrice.Text = text;
                txtUnitPrice.SelectionStart = text.Length;
            }
        }

        private void TxtUnitPrice_Leave(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtUnitPrice.Text, NumberStyles.Number,
                CultureInfo.InvariantCulture, out decimal value))
            {
                txtUnitPrice.Text = value.ToString("F2") + " ₽";
            }
        }

        private void FillFieldsForEdit()
        {
            cmbObject.SelectedValue = editingEstimate.ObjectId;
            txtMaterialName.Text = editingEstimate.MaterialName;
            cmbUnit.Text = editingEstimate.Unit;
            txtPlannedQuantity.Text = editingEstimate.PlannedQuantity.ToString("F2");
            txtActualQuantity.Text = editingEstimate.ActualQuantity.ToString("F2");
            txtUnitPrice.Text = editingEstimate.UnitPrice.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',') + " ₽";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbObject.SelectedValue == null || string.IsNullOrWhiteSpace(txtMaterialName.Text))
            {
                MessageBox.Show("Выберите объект и введите материал!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(txtPlannedQuantity.Text, out decimal plannedQty) || plannedQty <= 0 ||
                !decimal.TryParse(txtActualQuantity.Text, out decimal actualQty) || actualQty < 0)
            {
                MessageBox.Show("Введите корректные числовые значения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string priceText = txtUnitPrice.Text.Replace("₽", "").Replace(" ", "").Trim();

            if (!decimal.TryParse(priceText, NumberStyles.Number,
                CultureInfo.InvariantCulture, out decimal unitPrice) || unitPrice <= 0)
            {
                MessageBox.Show("Введите корректную цену (например: 150.50 или 150,50)!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var estimate = editingEstimate ?? new Estimate();
            estimate.ObjectId = (int)cmbObject.SelectedValue;
            estimate.MaterialName = txtMaterialName.Text.Trim();
            estimate.Unit = cmbUnit.Text;
            estimate.PlannedQuantity = plannedQty;
            estimate.ActualQuantity = actualQty;
            estimate.UnitPrice = unitPrice;

            if (editingEstimate == null) repository.AddEstimate(estimate);
            else repository.UpdateEstimate(editingEstimate);

            MessageBox.Show("Смета успешно сохранена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK; Close();
        }
    }

    // Форма отчета о перерасходах материалов
    public class OverrunReportForm : Form
    {
        public OverrunReportForm(dynamic overruns)
        {
            this.Text = "Отчет о перерасходах материалов";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            var grid = new DataGridView
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

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Объект", DataPropertyName = "ObjectName", FillWeight = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Материал", DataPropertyName = "MaterialName", FillWeight = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "План. кол-во", DataPropertyName = "PlannedQty", FillWeight = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Факт. кол-во", DataPropertyName = "ActualQty", FillWeight = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Перерасход (кол-во)", DataPropertyName = "OverrunQty", FillWeight = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Перерасход (стоимость)", DataPropertyName = "OverrunCost", FillWeight = 130 });

            grid.DataSource = overruns;
            this.Controls.Add(grid);
        }
    }
}