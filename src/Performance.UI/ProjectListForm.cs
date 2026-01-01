using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using Performance.UI;

namespace Performance
{
    public partial class ProjectListForm : BaseForm
    {
        private readonly IProjectService _projectService;
        private readonly IServiceProvider _provider;
        private DataGridView _grid = new DataGridView();
        private BindingSource _binding = new BindingSource();
        private TextBox _txtSearch = new TextBox();
        private Button _btnAdd, _btnEdit, _btnDelete;
        private FlowLayoutPanel _gallery = new FlowLayoutPanel();
        private ProjectEntity? _selectedProject;

        public ProjectListForm(IProjectService projectService, IServiceProvider provider)
        {
            _projectService = projectService;
            _provider = provider;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "ProjectListForm";
            this.Text = "Projects";
            this.StartPosition = FormStartPosition.CenterParent;

            // gallery view
            _gallery.Dock = DockStyle.Fill;
            _gallery.AutoScroll = true;
            _gallery.FlowDirection = FlowDirection.LeftToRight;
            _gallery.WrapContents = true;
            _gallery.Padding = new Padding(12);
            _gallery.BackColor = Color.FromArgb(250, 250, 250);

            // top panel for search
            var topPanel = new Panel() { Dock = DockStyle.Top, Height = 40 };
            _txtSearch.Left = 8; _txtSearch.Top = 8; _txtSearch.Width = 400; 
            try { _txtSearch.PlaceholderText = "Search projects..."; } catch { }
            _txtSearch.TextChanged += async (s, e) => await RefreshList(_txtSearch.Text);
            topPanel.Controls.Add(_txtSearch);
            UiHelpers.StyleGrid(_grid);

            // bottom panel for action buttons
            var bottomPanel = new Panel() { Dock = DockStyle.Bottom, Height = 56 };
            _btnAdd = new Button() { Text = "Add", Left = 8, Top = 12, Width = 80 };
            _btnEdit = new Button() { Text = "Edit", Left = 96, Top = 12, Width = 80, Enabled = false };
            _btnDelete = new Button() { Text = "Delete", Left = 184, Top = 12, Width = 80, Enabled = false };
            _btnAdd.Click += async (s, e) => await Add_Click();
            _btnEdit.Click += async (s, e) => await Edit_Click();
            _btnDelete.Click += async (s, e) => await Delete_Click();
            UiHelpers.ApplyButtonStyle(_btnAdd);
            UiHelpers.ApplyButtonStyle(_btnEdit);
            UiHelpers.ApplyButtonStyle(_btnDelete);
            bottomPanel.Controls.Add(_btnAdd);
            bottomPanel.Controls.Add(_btnEdit);
            bottomPanel.Controls.Add(_btnDelete);

            // add in order: topPanel, gallery, bottomPanel
            this.Controls.Add(bottomPanel);
            this.Controls.Add(_gallery);
            this.Controls.Add(topPanel);

            // gallery item click will open tasks
            this.Load += async (s, e) => await ProjectListForm_Load();
            this.ResumeLayout(false);
        }

        private async Task ProjectListForm_Load()
        {
            await RefreshList();
        }

        private async Task RefreshList(string? filter = null)
        {
            try
            {
                var projects = await _projectService.ListAsync();
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filter = filter.Trim().ToLowerInvariant();
                    projects = projects.FindAll(p => (p.Name ?? string.Empty).ToLowerInvariant().Contains(filter) || (p.Description ?? string.Empty).ToLowerInvariant().Contains(filter));
                }
                _binding.DataSource = projects;
                PopulateGallery(projects);
                UpdateButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load projects: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task Add_Click()
        {
            using var scope = _provider.CreateScope();
            var form = scope.ServiceProvider.GetRequiredService<ProjectEditForm>();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await RefreshList();
            }
        }

        private async Task Edit_Click()
        {
            if (_selectedProject == null) return;
            var project = _selectedProject;
            using var scope = _provider.CreateScope();
            var form = scope.ServiceProvider.GetRequiredService<ProjectEditForm>();
            form.LoadProject(project);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await RefreshList();
            }
        }

        private async Task Delete_Click()
        {
            if (_selectedProject == null) return;
            var project = _selectedProject;
            var r = MessageBox.Show($"Delete project '{project.Name}'?","Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;
            try
            {
                await _projectService.DeleteAsync(project.Id);
                await RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete project: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCardClick(object? sender, EventArgs e)
        {
            if (sender is Control c && c.Tag is ProjectEntity proj)
            {
                _selectedProject = proj;
                UpdateButtons();
                using var scope = _provider.CreateScope();
                var taskList = scope.ServiceProvider.GetRequiredService<TaskListForm>();
                taskList.SetProject(proj.Id);
                taskList.ShowDialog(this);
            }
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            var hasSelection = _grid.SelectedRows.Count > 0;
            _btnEdit.Enabled = hasSelection;
            _btnDelete.Enabled = hasSelection;
        }

        private void PopulateGallery(System.Collections.Generic.List<ProjectEntity> projects)
        {
            _gallery.Controls.Clear();
            foreach (var p in projects)
            {
                var card = new Panel() { Width = 260, Height = 120, Margin = new Padding(8), BackColor = Color.White, Tag = p };
                card.BorderStyle = BorderStyle.FixedSingle;
                var title = new Label() { Text = p.Name, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Left = 8, Top = 8, AutoSize = false, Width = 240 };
                var desc = new Label() { Text = p.Description ?? string.Empty, Left = 8, Top = 32, Width = 240, Height = 56, ForeColor = Color.FromArgb(90, 90, 90) };
                var btnOpen = new Button() { Text = "Open", Left = 8, Top = 90, Width = 60 };
                UiHelpers.ApplyButtonStyle(btnOpen);
                btnOpen.Click += OnCardClick;
                // clicking anywhere on card should act like open
                card.Click += OnCardClick;
                title.Click += OnCardClick;
                desc.Click += OnCardClick;
                // tag project for handlers
                card.Tag = p;
                title.Tag = p;
                desc.Tag = p;
                btnOpen.Tag = p;
                // purple accent bar
                var accent = new Panel() { Dock = DockStyle.Top, Height = 6, BackColor = Color.FromArgb(102, 51, 153) };
                card.Controls.Add(accent);
                card.Controls.Add(title);
                card.Controls.Add(desc);
                card.Controls.Add(btnOpen);
                _gallery.Controls.Add(card);
            }
        }
    }
}
