using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public class SitesManagementForm : Form
    {
        private readonly IConstructionRepository repository;
        private DataGridView sitesGrid;
        private Button btnAddSite;
        private ContextMenuStrip contextMenuStrip;
        private ComboBox cmbFilterDepartment;

        public SitesManagementForm(IConstructionRepository repo)
        {
            repository = repo;
            InitializeForm();
            LoadSites();
        }

        private void InitializeForm()
        {
            this.Text = "Управление участками";
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            sitesGrid = new DataGridView
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

            sitesGrid.DataBindingComplete += (s, e) => AddRowNumbers(sitesGrid);

            SetupSitesGrid();

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Редактировать", null, ContextMenu_Edit_Click);
            contextMenuStrip.Items.Add("Просмотр объектов", null, ContextMenu_ViewObjects_Click);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add("Удалить", null, ContextMenu_Delete_Click);

            sitesGrid.ContextMenuStrip = contextMenuStrip;

            btnAddSite = new Button
            {
                Text = "Добавить участок",
                Location = new Point(10, 10),
                Size = new Size(160, 30)
            };
            btnAddSite.Click += BtnAddSite_Click;

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
                Width = 205,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterDepartment.DisplayMember = "Name";
            cmbFilterDepartment.ValueMember = "Id";
            cmbFilterDepartment.Items.Add("Все управления");
            var allDepartments = repository.GetDepartments();
            foreach (var dept in allDepartments)
            {
                cmbFilterDepartment.Items.Add(dept);
            }
            cmbFilterDepartment.SelectedIndex = 0;
            cmbFilterDepartment.SelectedIndexChanged += (s, e) => LoadSites();

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke
            };
            topPanel.Controls.Add(btnAddSite);
            topPanel.Controls.Add(separator);
            topPanel.Controls.Add(cmbFilterDepartment);

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(sitesGrid);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);
        }

        private void SetupSitesGrid()
        {
            sitesGrid.Columns.Clear();

            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
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

            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Название",
                DataPropertyName = "Name",
                FillWeight = 1
            });

            // ✅ ДОБАВЛЕНА КОЛОНКА "АДРЕС"
            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Address",
                HeaderText = "Адрес",
                DataPropertyName = "Address",
                FillWeight = 1
            });

            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Department",
                HeaderText = "Управление",
                DataPropertyName = "DepartmentName",
                FillWeight = 1
            });
            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Manager",
                HeaderText = "Начальник",
                DataPropertyName = "ManagerName",
                FillWeight = 1
            });
            sitesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ObjectsCount",
                HeaderText = "Кол-во объектов",
                DataPropertyName = "ObjectsCount",
                FillWeight = 1
            });
        }

        private void LoadSites()
        {
            var sites = repository.GetSites().Select(s => new
            {
                s.Id,
                s.Name,
                // ✅ ПОЛУЧАЕМ АДРЕС ИЗ МОДЕЛИ
                Address = s.Address ?? "-",
                DepartmentName = repository.GetDepartment(s.DepartmentId)?.Name ?? "Неизвестно",
                ManagerName = repository.GetEmployee(s.SiteManagerId)?.FullName ?? "Не назначен",
                ObjectsCount = repository.GetObjectsBySite(s.Id).Count
            }).ToList();

            if (cmbFilterDepartment.SelectedIndex > 0 && cmbFilterDepartment.SelectedItem is ConstructionDepartment selectedDepartment)
            {
                sites = sites.Where(s => s.DepartmentName == selectedDepartment.Name).ToList();
            }

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

        private int GetSelectedSiteId()
        {
            if (sitesGrid.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Выберите участок.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return -1;
            }
            return (int)sitesGrid.CurrentRow.Cells["Id"].Value;
        }

        private void BtnAddSite_Click(object sender, EventArgs e)
        {
            var form = new AddSiteForm(repository);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadSites();
            }
        }

        private void ContextMenu_Edit_Click(object? sender, EventArgs e)
        {
            int siteId = GetSelectedSiteId();
            if (siteId < 0) return;

            var site = repository.GetSite(siteId);
            if (site == null)
            {
                MessageBox.Show("Не удалось загрузить данные участка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var form = new AddSiteForm(repository, site);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadSites();
            }
        }

        private void ContextMenu_Delete_Click(object? sender, EventArgs e)
        {
            int siteId = GetSelectedSiteId();
            if (siteId < 0) return;

            string siteName = sitesGrid.CurrentRow?.Cells["Name"].Value?.ToString() ?? "участок";

            var result = MessageBox.Show(
                $"Удалить участок \"{siteName}\" и все связанные с ним данные?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                repository.DeleteSite(siteId);
                LoadSites();
                MessageBox.Show("Участок удален.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ContextMenu_ViewObjects_Click(object? sender, EventArgs e)
        {
            int siteId = GetSelectedSiteId();
            if (siteId < 0) return;

            var form = new SiteObjectsForm(repository, siteId);
            form.ShowDialog();
        }
    }

    // Форма добавления/редактирования участка
    public class AddSiteForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly Site editingSite;
        private TextBox txtName;
        private TextBox txtStreet; // ← Поле для ввода улицы
        private ComboBox cmbDepartment;
        private ComboBox cmbManager;

        public AddSiteForm(IConstructionRepository repo) : this(repo, null) { }

        public AddSiteForm(IConstructionRepository repo, Site siteToEdit)
        {
            repository = repo;
            editingSite = siteToEdit;
            InitializeForm();

            if (editingSite != null)
            {
                FillFieldsForEdit();
            }
        }

        private void InitializeForm()
        {
            this.Text = editingSite == null ? "Добавить участок" : "Редактировать участок";
            this.Size = new Size(450, 260);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;

            // Название
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
            yPos += 35;

            // ✅ ПОЛЕ УЛИЦЫ
            var lblStreet = new Label
            {
                Text = "* Улица:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            txtStreet = new TextBox
            {
                Location = new Point(120, yPos),
                Width = 280
            };
            yPos += 35;

            // Управление
            var lblDepartment = new Label
            {
                Text = "* Управление:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            cmbDepartment = new ComboBox
            {
                Location = new Point(120, yPos),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDepartment.DataSource = repository.GetDepartments();
            cmbDepartment.DisplayMember = "Name";
            cmbDepartment.ValueMember = "Id";
            yPos += 35;

            // Начальник
            var lblManager = new Label
            {
                Text = "Начальник:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            cmbManager = new ComboBox
            {
                Location = new Point(120, yPos),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            LoadManagers();
            yPos += 35;

            // Кнопки
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
                lblStreet, txtStreet,
                lblDepartment, cmbDepartment,
                lblManager, cmbManager,
                btnSave, btnCancel
            });
        }

        private void LoadManagers()
        {
            var allManagers = repository.GetEmployees()
                .Where(e => e.Position == Position.SiteManager)
                .ToList();

            // Исключаем начальников, занятых на ДРУГИХ участках (кроме текущего при редактировании)
            var assignedManagerIds = repository.GetSites()
                .Where(s => s.SiteManagerId != 0 && s.Id != (editingSite?.Id ?? 0))
                .Select(s => s.SiteManagerId)
                .ToList();

            var availableManagers = allManagers.Where(m => !assignedManagerIds.Contains(m.Id)).ToList();

            var items = new System.Collections.Generic.List<Employee>();
            items.Add(new Employee { Id = 0, FullName = "-" }); // Опция "Не назначен"
            items.AddRange(availableManagers);

            cmbManager.DataSource = items;
            cmbManager.DisplayMember = "FullName";
            cmbManager.ValueMember = "Id";
        }

        private void FillFieldsForEdit()
        {
            txtName.Text = editingSite.Name;

            // ✅ ИЗВЛЕЧЕНИЕ УЛИЦЫ ИЗ ПОЛНОГО АДРЕСА
            if (!string.IsNullOrEmpty(editingSite.Address) && editingSite.Address.StartsWith("г. Русский, ул. "))
            {
                txtStreet.Text = editingSite.Address.Substring("г. Русский, ул. ".Length);
            }
            else
            {
                txtStreet.Text = editingSite.Address ?? "";
            }

            if (editingSite.DepartmentId > 0)
            {
                cmbDepartment.SelectedValue = editingSite.DepartmentId;
            }

            // ✅ УСТАНОВКА НАЧАЛЬНИКА (с проверкой наличия в списке)
            if (editingSite.SiteManagerId > 0)
            {
                var manager = repository.GetEmployee(editingSite.SiteManagerId);
                if (manager != null)
                {
                    var items = cmbManager.DataSource as System.Collections.Generic.List<Employee>;
                    if (items != null && !items.Any(e => e.Id == manager.Id))
                    {
                        // Если текущий начальник был исключен из списка (редкий случай гонки данных), добавляем его обратно
                        items.Add(manager);
                        cmbManager.DataSource = null;
                        cmbManager.DataSource = items;
                        cmbManager.DisplayMember = "FullName";
                        cmbManager.ValueMember = "Id";
                    }
                    cmbManager.SelectedValue = manager.Id;
                }
                else
                {
                    cmbManager.SelectedValue = 0;
                }
            }
            else
            {
                cmbManager.SelectedValue = 0;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string inputName = txtName.Text.Trim();
            string street = txtStreet.Text.Trim();

            // Проверка названия
            if (string.IsNullOrWhiteSpace(inputName) || inputName.Length < 3)
            {
                MessageBox.Show("Название участка должно содержать не менее 3 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ✅ ПРОВЕРКА УЛИЦЫ (минимум 2 символа)
            if (string.IsNullOrWhiteSpace(street) || street.Length < 2)
            {
                MessageBox.Show("Название улицы должно содержать не менее 2 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbDepartment.SelectedValue == null)
            {
                MessageBox.Show("Выберите управление!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbManager.SelectedValue == null)
            {
                MessageBox.Show("Произошла ошибка при загрузке списка начальников!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int siteManagerId = (int)cmbManager.SelectedValue;

            // ✅ ФОРМИРОВАНИЕ ПОЛНОГО АДРЕСА
            string fullAddress = $"г. Русский, ул. {street}";

            if (editingSite == null)
            {
                var site = new Site
                {
                    Name = inputName,
                    Address = fullAddress, // ← Сохраняем полный адрес
                    DepartmentId = (int)cmbDepartment.SelectedValue,
                    SiteManagerId = siteManagerId
                };
                repository.AddSite(site);
            }
            else
            {
                editingSite.Name = inputName;
                editingSite.Address = fullAddress; // ← Обновляем полный адрес
                editingSite.DepartmentId = (int)cmbDepartment.SelectedValue;
                editingSite.SiteManagerId = siteManagerId;
                repository.UpdateSite(editingSite);
            }

            MessageBox.Show("Участок успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // Форма просмотра объектов участка
    public class SiteObjectsForm : Form
    {
        private readonly IConstructionRepository repository;
        private readonly int siteId;
        private DataGridView objectsGrid;

        public SiteObjectsForm(IConstructionRepository repo, int siteId)
        {
            repository = repo;
            this.siteId = siteId;
            InitializeForm();
            LoadObjects();
        }

        private void InitializeForm()
        {
            var site = repository.GetSite(siteId);
            this.Text = $"Объекты участка: {site?.Name ?? "Неизвестно"}";
            this.Size = new Size(900, 500);
            this.StartPosition = FormStartPosition.CenterParent;

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
            this.Controls.Add(objectsGrid);
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
                HeaderText = "Название",
                DataPropertyName = "Name",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Тип",
                DataPropertyName = "Type",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Заказчик",
                DataPropertyName = "Customer",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Начало",
                DataPropertyName = "StartDate",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Окончание",
                DataPropertyName = "EndDate",
                FillWeight = 1
            });
            objectsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Статус",
                DataPropertyName = "Status",
                FillWeight = 1
            });
        }

        private void LoadObjects()
        {
            var objects = repository.GetObjectsBySite(siteId).Select(o => new
            {
                o.Name,
                Type = o.Type.ToString(),
                Customer = repository.GetCustomer(o.CustomerId)?.Name ?? "Неизвестно",
                StartDate = o.StartDate.ToString("dd.MM.yyyy"),
                EndDate = o.PlannedEndDate.ToString("dd.MM.yyyy"),
                Status = o.ActualEndDate.HasValue ? "Завершен" : "В работе"
            }).ToList();

            objectsGrid.DataSource = objects;
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