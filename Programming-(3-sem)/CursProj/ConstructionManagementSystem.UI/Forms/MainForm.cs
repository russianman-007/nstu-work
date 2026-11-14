using System;
using System.Windows.Forms;
using System.Drawing;
using ConstructionManagementSystem.Repositories;
using ConstructionManagementSystem.Models;

namespace ConstructionManagementSystem.Forms
{
    public partial class MainForm : Form
    {
        private IConstructionRepository repository;
        private TabControl mainTabControl;

        public MainForm()
        {
            repository = new ConstructionRepository();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main Form
            this.Text = "Система управления строительной организацией";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;

            // TabControl
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Вкладки
            var departmentsTab = new TabPage("Строительные управления");
            var sitesTab = new TabPage("Участки");
            var objectsTab = new TabPage("Объекты");
            var personnelTab = new TabPage("Сотрудники");
            var brigadesTab = new TabPage("Бригады");
            var equipmentTab = new TabPage("Техника");
            var schedulesTab = new TabPage("Графики работ");
            var estimatesTab = new TabPage("Сметы");
            var reportsTab = new TabPage("Отчеты");

            mainTabControl.TabPages.Add(departmentsTab);
            mainTabControl.TabPages.Add(sitesTab);
            mainTabControl.TabPages.Add(objectsTab);
            mainTabControl.TabPages.Add(personnelTab);
            mainTabControl.TabPages.Add(brigadesTab);
            mainTabControl.TabPages.Add(equipmentTab);
            mainTabControl.TabPages.Add(schedulesTab);
            mainTabControl.TabPages.Add(estimatesTab);
            mainTabControl.TabPages.Add(reportsTab);

            // Настройка вкладок
            SetupDepartmentsTab(departmentsTab);
            SetupSitesTab(sitesTab);
            SetupObjectsTab(objectsTab);
            SetupPersonnelTab(personnelTab);
            SetupBrigadesTab(brigadesTab);
            SetupEquipmentTab(equipmentTab);
            SetupSchedulesTab(schedulesTab);
            SetupEstimatesTab(estimatesTab);
            SetupReportsTab(reportsTab);

            this.Controls.Add(mainTabControl);
            this.ResumeLayout(false);
        }

        // ================== ВКЛАДКА СТРОИТЕЛЬНЫХ УПРАВЛЕНИЙ ==================
        private void SetupDepartmentsTab(TabPage tabPage)
        {
            var departmentsForm = new DepartmentsManagementForm(repository)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            tabPage.Controls.Clear();
            tabPage.Controls.Add(departmentsForm);
            departmentsForm.Show();
        }

        // ================== ВКЛАДКА УЧАСТКОВ ==================
        private void SetupSitesTab(TabPage tabPage)
        {
            var sitesForm = new SitesManagementForm(repository)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            tabPage.Controls.Clear();
            tabPage.Controls.Add(sitesForm);
            sitesForm.Show();
        }

        // ================== ВКЛАДКА ОБЪЕКТОВ ==================
        private void SetupObjectsTab(TabPage tabPage)
        {
            var objectsForm = new ObjectsManagementForm(repository)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            tabPage.Controls.Clear();
            tabPage.Controls.Add(objectsForm);
            objectsForm.Show();
        }

        // ================== ВКЛАДКА ПЕРСОНАЛА ==================
        private void SetupPersonnelTab(TabPage tabPage)
        {
            var personnelForm = new PersonnelManagementForm(repository)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            tabPage.Controls.Clear();
            tabPage.Controls.Add(personnelForm);
            personnelForm.Show();
        }

        // ================== ВКЛАДКА БРИГАД ==================
        private void SetupBrigadesTab(TabPage tabPage)
        {
            var brigadesForm = new BrigadesManagementForm(repository)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            tabPage.Controls.Clear();
            tabPage.Controls.Add(brigadesForm);
            brigadesForm.Show();
        }

        // ================== ВКЛАДКА ТЕХНИКИ ==================
        private void SetupEquipmentTab(TabPage tabPage)
        {
            var equipmentForm = new EquipmentManagementForm(repository)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            tabPage.Controls.Clear();
            tabPage.Controls.Add(equipmentForm);
            equipmentForm.Show();
        }

        // ================== ВКЛАДКА ГРАФИКОВ РАБОТ ==================
        private void SetupSchedulesTab(TabPage tabPage)
        {
            var schedulesForm = new WorkSchedulesForm(repository)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            tabPage.Controls.Clear();
            tabPage.Controls.Add(schedulesForm);
            schedulesForm.Show();
        }

        // ================== ВКЛАДКА СМЕТ ==================
        private void SetupEstimatesTab(TabPage tabPage)
        {
            var estimatesForm = new EstimatesManagementForm(repository)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            tabPage.Controls.Clear();
            tabPage.Controls.Add(estimatesForm);
            estimatesForm.Show();
        }

        // ================== ВКЛАДКА ОТЧЕТОВ ==================
        private void SetupReportsTab(TabPage tabPage)
        {
            var reportsForm = new ReportsForm(repository)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            tabPage.Controls.Clear();
            tabPage.Controls.Add(reportsForm);
            reportsForm.Show();
        }
    }
}