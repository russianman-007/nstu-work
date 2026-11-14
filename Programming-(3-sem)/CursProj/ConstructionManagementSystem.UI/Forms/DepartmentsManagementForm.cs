using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public class DepartmentsManagementForm : Form
    {
        private readonly IConstructionRepository repository;
        private DataGridView departmentsGrid;
        private Button btnAddDepartment;
        private ContextMenuStrip contextMenuStrip;

        public DepartmentsManagementForm(IConstructionRepository repo)
        {
            repository = repo;
            InitializeForm();
            LoadDepartments();
        }

        private void InitializeForm()
        {
            this.Text = "Управление строительными управлениями";
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            departmentsGrid = new DataGridView
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

            departmentsGrid.DataBindingComplete += (s, e) => AddRowNumbers(departmentsGrid);

            SetupDepartmentsGrid();

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Редактировать", null, ContextMenu_Edit_Click);
            contextMenuStrip.Items.Add("Просмотр участков", null, ContextMenu_ViewSites_Click);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add("Удалить", null, ContextMenu_Delete_Click);

            departmentsGrid.ContextMenuStrip = contextMenuStrip;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke
            };

            btnAddDepartment = new Button
            {
                Text = "Добавить управление",
                Location = new Point(10, 10),
                Size = new Size(160, 30)
            };
            btnAddDepartment.Click += BtnAddDepartment_Click;

            topPanel.Controls.Add(btnAddDepartment);

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(departmentsGrid);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);
        }

        private void SetupDepartmentsGrid()
        {
            departmentsGrid.Columns.Clear();

            departmentsGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            departmentsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            departmentsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Название",
                DataPropertyName = "Name",
                FillWeight = 1
            });
            departmentsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Address",
                HeaderText = "Адрес",
                DataPropertyName = "Address",
                FillWeight = 1
            });
            departmentsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Head",
                HeaderText = "Руководитель",
                DataPropertyName = "HeadName",
                FillWeight = 1
            });
            departmentsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SitesCount",
                HeaderText = "Количество участков",
                DataPropertyName = "SitesCount",
                FillWeight = 1
            });
            departmentsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ObjectsCount",
                HeaderText = "Количество объектов",
                DataPropertyName = "ObjectsCount",
                FillWeight = 1
            });
        }

        private void LoadDepartments()
        {
            var departments = repository.GetDepartments().Select(d => new
            {
                d.Id,
                d.Name,
                d.Address,
                // Если ID руководителя 0 или null, выведется "Не назначен"
                HeadName = d.DepartmentHeadId > 0
                    ? (repository.GetEmployee(d.DepartmentHeadId)?.FullName ?? "Не назначен")
                    : "Не назначен",
                SitesCount = repository.GetSitesByDepartment(d.Id).Count,
                ObjectsCount = repository.GetObjectsByDepartment(d.Id).Count
            }).ToList();

            departmentsGrid.DataSource = departments;
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

        private int GetSelectedDepartmentId()
        {
            if (departmentsGrid.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Выберите управление.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return -1;
            }

            return (int)departmentsGrid.CurrentRow.Cells["Id"].Value;
        }

        private void BtnAddDepartment_Click(object sender, EventArgs e)
        {
            var form = new AddDepartmentForm(repository);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadDepartments();
            }
        }

        private void ContextMenu_Edit_Click(object? sender, EventArgs e)
        {
            int departmentId = GetSelectedDepartmentId();
            if (departmentId < 0) return;

            var department = repository.GetDepartment(departmentId);
            if (department == null)
            {
                MessageBox.Show("Не удалось загрузить данные управления.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var form = new AddDepartmentForm(repository, department);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadDepartments();
            }
        }

        private void ContextMenu_Delete_Click(object? sender, EventArgs e)
        {
            int departmentId = GetSelectedDepartmentId();
            if (departmentId < 0) return;

            string departmentName = departmentsGrid.CurrentRow?.Cells["Name"].Value?.ToString() ?? "управление";

            var result = MessageBox.Show(
                $"Удалить строительное управление \"{departmentName}\" и все связанные с ним данные?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                repository.DeleteDepartment(departmentId);
                LoadDepartments();
                MessageBox.Show("Управление удалено.", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ContextMenu_ViewSites_Click(object? sender, EventArgs e)
        {
            int departmentId = GetSelectedDepartmentId();
            if (departmentId < 0) return;

            var form = new DepartmentSitesForm(repository, departmentId);
            form.ShowDialog();
        }
    }

    // Форма добавления/редактирования управления
    public class AddDepartmentForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly ConstructionDepartment editingDepartment;
        private TextBox txtName;
        private TextBox txtStreet;
        private ComboBox cmbHead;

        public AddDepartmentForm(IConstructionRepository repo)
            : this(repo, null) { }

        public AddDepartmentForm(IConstructionRepository repo, ConstructionDepartment departmentToEdit)
        {
            repository = repo;
            editingDepartment = departmentToEdit;
            InitializeForm();

            if (editingDepartment != null)
            {
                FillFieldsForEdit();
            }
        }

        private void InitializeForm()
        {
            this.Text = editingDepartment == null ? "Добавить управление" : "Редактировать управление";
            this.Size = new Size(450, 225);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblName = new Label
            {
                Text = "Название:",
                Location = new Point(10, 20),
                Size = new Size(100, 20)
            };
            txtName = new TextBox
            {
                Location = new Point(120, 20),
                Width = 280
            };

            var lblStreet = new Label
            {
                Text = "Адрес:",
                Location = new Point(10, 55),
                Size = new Size(100, 20)
            };
            txtStreet = new TextBox
            {
                Location = new Point(120, 55),
                Width = 280
            };

            var lblHead = new Label
            {
                Text = "Руководитель:",
                Location = new Point(10, 90),
                Size = new Size(100, 20)
            };
            cmbHead = new ComboBox
            {
                Location = new Point(120, 90),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            LoadLeaders();

            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(120, 135),
                Size = new Size(100, 30)
            };
            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(240, 135),
                Size = new Size(100, 30)
            };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[]
            {
                lblName, txtName,
                lblStreet, txtStreet,
                lblHead, cmbHead,
                btnSave, btnCancel
            });
        }

        private void LoadLeaders()
        {
            // Получаем всех сотрудников с должностью "Руководитель"
            var allHeads = repository.GetEmployees()
                .Where(e => e.Position == Position.DepartmentHead)
                .ToList();

            // ✅ ИСПРАВЛЕНИЕ: Исключаем руководителей, назначенных на ДРУГИЕ управления.
            // Текущее редактируемое управление (editingDepartment) мы исключаем из фильтра,
            // поэтому текущий руководитель останется в списке.
            var assignedHeadIds = repository.GetDepartments()
                .Where(d => d.DepartmentHeadId != 0 && d.Id != (editingDepartment?.Id ?? 0))
                .Select(d => d.DepartmentHeadId)
                .ToList();

            var availableHeads = allHeads.Where(h => !assignedHeadIds.Contains(h.Id)).ToList();

            var items = new System.Collections.Generic.List<Employee>();
            items.Add(new Employee { Id = 0, FullName = "-" });
            items.AddRange(availableHeads);

            cmbHead.DataSource = items;
            cmbHead.DisplayMember = "FullName";
            cmbHead.ValueMember = "Id";

            // ✅ Теперь можно спокойно устанавливать значение в FillFieldsForEdit,
            // так как текущий руководитель гарантированно есть в списке items.
        }

        private void FillFieldsForEdit()
        {
            txtName.Text = editingDepartment.Name;

            string fullAddress = editingDepartment.Address;
            if (fullAddress.StartsWith("г. Русский, ул. "))
            {
                txtStreet.Text = fullAddress.Substring("г. Русский, ул. ".Length);
            }
            else
            {
                txtStreet.Text = fullAddress;
            }

            if (editingDepartment.DepartmentHeadId > 0)
            {
                var head = repository.GetEmployee(editingDepartment.DepartmentHeadId);
                if (head != null)
                {
                    var items = cmbHead.DataSource as System.Collections.Generic.List<Employee>;
                    if (items != null && !items.Any(e => e.Id == head.Id))
                    {
                        items.Add(head);
                        cmbHead.DataSource = null;
                        cmbHead.DataSource = items;
                        cmbHead.DisplayMember = "FullName";
                        cmbHead.ValueMember = "Id";
                    }
                    cmbHead.SelectedValue = head.Id;
                }
                else
                {
                    cmbHead.SelectedValue = 0;
                }
            }
            else
            {
                cmbHead.SelectedValue = 0;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string inputName = txtName.Text.Trim();
            string street = txtStreet.Text.Trim();

            // ✅ Обработка: делаем первую букву улицы заглавной
            if (!string.IsNullOrEmpty(street))
            {
                street = char.ToUpper(street[0]) + (street.Length > 1 ? street.Substring(1).ToLower() : "");
            }

            if (string.IsNullOrWhiteSpace(inputName) || inputName.Length < 2)
            {
                MessageBox.Show("Название управления должно содержать не менее 2 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(street) || street.Length < 2)
            {
                MessageBox.Show("Название улицы должно содержать не менее 2 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbHead.SelectedValue == null)
            {
                MessageBox.Show("Произошла ошибка при загрузке списка руководителей!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int departmentHeadId = (int)cmbHead.SelectedValue;

            string finalName = inputName;

            if (editingDepartment == null)
            {
                string[] prefixes = { "АО", "ОАО", "ООО" };
                Random rand = new Random();
                string prefix = prefixes[rand.Next(prefixes.Length)];
                finalName = $"{prefix} \"{inputName}\"";
            }

            string fullAddress = $"г. Русский, ул. {street}";

            if (editingDepartment == null)
            {
                var department = new ConstructionDepartment
                {
                    Name = finalName,
                    Address = fullAddress,
                    DepartmentHeadId = departmentHeadId
                };
                repository.AddDepartment(department);
            }
            else
            {
                editingDepartment.Name = finalName;
                editingDepartment.Address = fullAddress;
                editingDepartment.DepartmentHeadId = departmentHeadId;
                repository.UpdateDepartment(editingDepartment);
            }

            MessageBox.Show("Управление успешно сохранено!", "Успех",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // Форма просмотра участков управления
    public class DepartmentSitesForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int departmentId;
        private DataGridView sitesGrid;

        public DepartmentSitesForm(IConstructionRepository repo, int deptId)
        {
            repository = repo;
            departmentId = deptId;
            InitializeForm();
            LoadSites();
        }

        private void InitializeForm()
        {
            var department = repository.GetDepartment(departmentId);
            this.Text = $"Участки управления: {department?.Name ?? "Неизвестно"}";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            sitesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,  // ← ИСПРАВЛЕНО
                BackgroundColor = Color.White
            };

            sitesGrid.DataBindingComplete += (s, e) => AddRowNumbers(sitesGrid);

            SetupSitesGrid();
            this.Controls.Add(sitesGrid);
        }

        private void SetupSitesGrid()
        {
            sitesGrid.Columns.Clear();

            // Колонка "№" - фиксированная ширина
            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "№",
                Width = 25,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,  // ← Не растягивать
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.White
                }
            });

            // Остальные колонки - заполняют пространство
            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Название",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill  // ← Заполнять
            });
            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Начальник",
                DataPropertyName = "ManagerName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill  // ← Заполнять
            });
            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Количество объектов",
                DataPropertyName = "ObjectsCount",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill  // ← Заполнять
            });
            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Количество бригад",
                DataPropertyName = "BrigadesCount",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill  // ← Заполнять
            });
        }

        private void LoadSites()
        {
            var sites = repository.GetSitesByDepartment(departmentId).Select(s => new
            {
                s.Name,
                ManagerName = repository.GetEmployee(s.SiteManagerId)?.FullName ?? "Не назначен",
                ObjectsCount = repository.GetObjectsBySite(s.Id).Count,
                BrigadesCount = repository.GetBrigades().Count(b =>
                    repository.GetObjectsBySite(s.Id).Any(o => o.Id == b.CurrentObjectId))
            }).ToList();

            sitesGrid.DataSource = sites;
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
    }
}