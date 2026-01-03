using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using Microsoft.Extensions.DependencyInjection;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.UI;

namespace Performance
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly IProjectService _projectService;
        private readonly IServiceProvider _serviceProvider;
        private UserEntity? _authenticatedUser;
        private FlowLayoutPanel _gallery = new FlowLayoutPanel();
        private TextBox _txtSearch = new TextBox();
        private Label _lblWelcome = new Label();
        private Button _btnAddProject = new Button();
        private Button _btnEditProject = new Button();
        private Button _btnDeleteProject = new Button();
        private Button _btnUserManagement = new Button();
        private Button _btnChangePassword = new Button();
        private Button _btnMyProfile = new Button();
        private ProjectEntity? _selectedProject;
        private DashboardStatsPanel? _statsPanel;
        private System.Threading.CancellationTokenSource? _searchCancellation; 
        private System.Threading.Timer? _searchTimer;  

        public MainForm(IProjectService projectService, IServiceProvider serviceProvider)
        {
            _projectService = projectService;
            _serviceProvider = serviceProvider;
            _statsPanel = new DashboardStatsPanel(serviceProvider);
            
            InitializeSkin();
            InitializeComponent();
        }

        public void SetAuthenticatedUser(UserEntity user)
        {
            _authenticatedUser = user;
        }

        private void InitializeSkin()
        {
            // Disable dark theme - use custom colors
            SkinManager.EnableFormSkins();
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1300, 850);
            this.Name = "MainForm";
            this.Text = "Performance - Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = UiColors.Background;

            // Top panel with welcome and search
            var topPanel = new Panel() 
            { 
                Dock = DockStyle.Top, 
                Height = 100, 
                BackColor = UiColors.DarkGreen
            };
            
            _lblWelcome = new Label() 
            { 
                Text = "Welcome", 
                Font = new Font("Segoe UI", 16F, FontStyle.Bold), 
                ForeColor = Color.White, 
                Left = 25, 
                Top = 15, 
                AutoSize = true 
            };
            
            var lblSearchTitle = new Label()
            {
                Text = "Search Projects:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = UiColors.LightGreen,
                Left = 25,
                Top = 55,
                AutoSize = true
            };
            
            _txtSearch = new TextBox() 
            { 
                Left = 145, 
                Top = 53, 
                Width = 400, 
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.White,
                ForeColor = UiColors.PrimaryText
            };
            _txtSearch.TextChanged += (s, e) =>
            {
                _searchCancellation?.Cancel();
                _searchCancellation = new System.Threading.CancellationTokenSource();

                _searchTimer?.Dispose();
                _searchTimer = new System.Threading.Timer(async _ =>
                {
                    if (!_searchCancellation.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await this.Invoke(async () => await RefreshProjects(_txtSearch.Text));
                        }
                        catch { }
                    }
                }, null, 300, System.Threading.Timeout.Infinite);
            };
            topPanel.Controls.Add(_lblWelcome);
            topPanel.Controls.Add(lblSearchTitle);
            topPanel.Controls.Add(_txtSearch);

            // Gallery panel
            _gallery.Dock = DockStyle.Fill;
            _gallery.AutoScroll = true;
            _gallery.FlowDirection = FlowDirection.LeftToRight;
            _gallery.WrapContents = true;
            _gallery.Padding = new Padding(20);
            _gallery.BackColor = UiColors.Background;

            // Bottom panel with action buttons
            var bottomPanel = new Panel() 
            { 
                Dock = DockStyle.Bottom, 
                Height = 70, 
                BackColor = UiColors.MediumGreen
            };
            
            _btnAddProject = new Button() 
            { 
                Text = "Add Project", 
                Left = 25, 
                Top = 18, 
                Width = 130, 
                Height = 35 
            };
            
            _btnEditProject = new Button() 
            { 
                Text = "Edit", 
                Left = 165, 
                Top = 18, 
                Width = 100, 
                Height = 35, 
                Enabled = false 
            };
            
            _btnDeleteProject = new Button() 
            { 
                Text = "Delete", 
                Left = 275, 
                Top = 18, 
                Width = 100, 
                Height = 35, 
                Enabled = false 
            };
            
            _btnUserManagement = new Button() 
            { 
                Text = "Manage Users", 
                Left = 385, 
                Top = 18, 
                Width = 150, 
                Height = 35, 
                Visible = false 
            };

            _btnChangePassword = new Button() 
            { 
                Text = "Change Password", 
                Left = 545, 
                Top = 18, 
                Width = 160, 
                Height = 35 
            };

            _btnMyProfile = new Button() 
            { 
                Text = "My Profile", 
                Left = 715, 
                Top = 18, 
                Width = 130, 
                Height = 35 
            };
            
            _btnAddProject.Click += async (s, e) => await AddProject_Click();
            _btnEditProject.Click += async (s, e) => await EditProject_Click();
            _btnDeleteProject.Click += async (s, e) => await DeleteProject_Click();
            _btnUserManagement.Click += (s, e) => OpenUserManagement();
            _btnChangePassword.Click += async (s, e) => await ChangeMyPassword_Click();
            _btnMyProfile.Click += (s, e) => OpenMyProfile();

            UiHelpers.ApplyButtonStyle(_btnAddProject);
            UiHelpers.ApplyButtonStyle(_btnEditProject);
            UiHelpers.ApplyButtonStyle(_btnDeleteProject);
            UiHelpers.ApplyButtonStyle(_btnUserManagement);
            UiHelpers.ApplyButtonStyle(_btnChangePassword);
            UiHelpers.ApplyButtonStyle(_btnMyProfile);

            bottomPanel.Controls.Add(_btnAddProject);
            bottomPanel.Controls.Add(_btnEditProject);
            bottomPanel.Controls.Add(_btnDeleteProject);
            bottomPanel.Controls.Add(_btnUserManagement);
            bottomPanel.Controls.Add(_btnChangePassword);
            bottomPanel.Controls.Add(_btnMyProfile);

            this.Controls.Add(bottomPanel);
            this.Controls.Add(_gallery);
            this.Controls.Add(topPanel);

            if (_statsPanel != null)
            {
                this.Controls.Add(_statsPanel);
            }

            this.Load += async (s, e) => await MainForm_Load();
            this.ResumeLayout(false);
        }

        private async Task MainForm_Load()
        {
            if (_authenticatedUser != null)
            {
                var roleText = _authenticatedUser.Role == UserRole.Manager ? "Manager" : "Employee";
                _lblWelcome.Text = $"Welcome, {_authenticatedUser.UserName} ({roleText})";
                
                // Role-based button visibility
                if (_authenticatedUser.Role == UserRole.Manager)
                {
                    _btnUserManagement.Visible = true;
                }
                else // Employee
                {
                    // Hide project management buttons for employees
                    _btnAddProject.Visible = false;
                    _btnEditProject.Visible = false;
                    _btnDeleteProject.Visible = false;
                    
                    // Move Change Password and My Profile to the left
                    _btnChangePassword.Left = 25;
                    _btnMyProfile.Left = 195;
                }
            }
            
            await RefreshProjects();

            _statsPanel?.SetCurrentUser(_authenticatedUser);
            await _statsPanel.RefreshStatistics();
        }

        private async Task RefreshProjects(string? filter = null)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();

                var projects = await projectService.ListAsync(includeTasks: true);
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filter = filter.Trim().ToLowerInvariant();
                    projects = projects.Where(p => 
                        (p.Name ?? string.Empty).ToLowerInvariant().Contains(filter) || 
                        (p.Description ?? string.Empty).ToLowerInvariant().Contains(filter)).ToList();
                }
                PopulateGallery(projects);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load projects: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateGallery(List<ProjectEntity> projects)
        {
            _gallery.Controls.Clear();
            foreach (var p in projects)
            {
                var card = CreateProjectCard(p);
                _gallery.Controls.Add(card);
            }
        }

        private Panel CreateProjectCard(ProjectEntity project)
        {
            var card = new Panel() 
            { 
                Width = 300, 
                Height = 200, 
                Margin = new Padding(15), 
                BackColor = Color.White,
                Tag = project,
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Accent bar
            var accent = new Panel() 
            { 
                Dock = DockStyle.Top, 
                Height = 6, 
                BackColor = UiColors.DarkGreen 
            };
            
            // Title
            var title = new Label() 
            { 
                Text = project.Name, 
                Font = new Font("Segoe UI", 13F, FontStyle.Bold), 
                ForeColor = UiColors.PrimaryText,
                Left = 15, 
                Top = 20, 
                AutoSize = false, 
                Width = 270,
                Height = 30,
                Tag = project
            };
            
            // Description
            var desc = new Label() 
            { 
                Text = project.Description ?? "No description", 
                ForeColor = UiColors.SecondaryText,
                Font = new Font("Segoe UI", 9F),
                Left = 15, 
                Top = 55, 
                Width = 270, 
                Height = 70,
                Tag = project
            };
            
            // Task count
            var taskCount = new Label() 
            { 
                Text = $"{project.Tasks?.Count ?? 0} tasks", 
                ForeColor = UiColors.SecondaryText,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                Left = 15, 
                Top = 135,
                Tag = project
            };

            // Open button
            var btnOpen = new Button() 
            { 
                Text = "OPEN", 
                Left = 15, 
                Top = 160, 
                Width = 90, 
                Height = 30,
                Tag = project
            };
            UiHelpers.ApplyButtonStyle(btnOpen);

            // Hover effect for card
            card.MouseEnter += (s, e) => card.BackColor = UiColors.Lighten(Color.White, 0.5f);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;

            // Click handlers
            card.Click += (s, e) => OnCardClick(project);
            title.Click += (s, e) => OnCardClick(project);
            desc.Click += (s, e) => OnCardClick(project);
            taskCount.Click += (s, e) => OnCardClick(project);
            btnOpen.Click += (s, e) => OnCardClick(project);

            card.Controls.Add(accent);
            card.Controls.Add(title);
            card.Controls.Add(desc);
            card.Controls.Add(taskCount);
            card.Controls.Add(btnOpen);

            return card;
        }

        private async void OnCardClick(ProjectEntity project)
        {
            _selectedProject = project;
            
            // Only enable edit/delete for managers
            if (_authenticatedUser?.Role == UserRole.Manager)
            {
                _btnEditProject.Enabled = true;
                _btnDeleteProject.Enabled = true;
            }

            using var scope = _serviceProvider.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var taskList = new TaskListForm(taskService, _serviceProvider, userService);
            taskList.SetProject(project.Id);
            taskList.SetCurrentUser(_authenticatedUser);
            taskList.ShowDialog(this);
            
            await RefreshProjects(_txtSearch.Text);
            await _statsPanel.RefreshStatistics();
        }

        private async Task AddProject_Click()
        {
            using var scope = _serviceProvider.CreateScope();
            var form = scope.ServiceProvider.GetRequiredService<ProjectEditForm>();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await RefreshProjects(_txtSearch.Text);
                await _statsPanel.RefreshStatistics();
            }
        }

        private async Task EditProject_Click()
        {
            if (_selectedProject == null) return;
            using var scope = _serviceProvider.CreateScope();
            var form = scope.ServiceProvider.GetRequiredService<ProjectEditForm>();
            form.LoadProject(_selectedProject);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await RefreshProjects(_txtSearch.Text);
                await _statsPanel.RefreshStatistics();
            }
        }

        private async Task DeleteProject_Click()
        {
            if (_selectedProject == null) return;
            var result = MessageBox.Show(
                $"Delete project '{_selectedProject.Name}'?\n\nThis will also delete all tasks in this project.",
                "Delete Project", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;
            
            try
            {
                await _projectService.DeleteAsync(_selectedProject.Id);
                _selectedProject = null;
                _btnEditProject.Enabled = false;
                _btnDeleteProject.Enabled = false;
                await RefreshProjects(_txtSearch.Text);
                await _statsPanel.RefreshStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete project: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenUserManagement()
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var form = new UserManagementForm(userService, _authenticatedUser, _serviceProvider);
            form.ShowDialog(this);
        }

        private void OpenMyProfile()
        {
            if (_authenticatedUser == null) return;

            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
            
            var form = new UserProfileForm(userService, taskService, _authenticatedUser);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // Refresh user data after profile update
                Task.Run(async () =>
                {
                    var updatedUser = await userService.GetAsync(_authenticatedUser.Id);
                    if (updatedUser != null)
                    {
                        _authenticatedUser = updatedUser;
                        this.Invoke((MethodInvoker)delegate
                        {
                            var roleText = _authenticatedUser.Role == UserRole.Manager ? "Manager" : "Employee";
                            _lblWelcome.Text = $"Welcome, {_authenticatedUser.UserName} ({roleText})";
                        });
                    }
                });
            }
        }

        private async Task ChangeMyPassword_Click()
        {
            if (_authenticatedUser == null) return;

            using var dialog = new Form()
            {
                Text = "Change My Password",
                Width = 450,
                Height = 280,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = UiColors.Background,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblCurrent = new Label()
            {
                Text = "Current Password:",
                Left = 20,
                Top = 20,
                ForeColor = UiColors.PrimaryText,
                AutoSize = true
            };
            var txtCurrent = new TextBox()
            {
                Left = 20,
                Top = 45,
                Width = 390,
                UseSystemPasswordChar = true
            };
            UiHelpers.StyleTextBox(txtCurrent);

            var lblNew = new Label()
            {
                Text = "New Password:",
                Left = 20,
                Top = 80,
                ForeColor = UiColors.PrimaryText,
                AutoSize = true
            };
            var txtNew = new TextBox()
            {
                Left = 20,
                Top = 105,
                Width = 390,
                UseSystemPasswordChar = true
            };
            UiHelpers.StyleTextBox(txtNew);

            var lblConfirm = new Label()
            {
                Text = "Confirm New Password:",
                Left = 20,
                Top = 140,
                ForeColor = UiColors.PrimaryText,
                AutoSize = true
            };
            var txtConfirm = new TextBox()
            {
                Left = 20,
                Top = 165,
                Width = 390,
                UseSystemPasswordChar = true
            };
            UiHelpers.StyleTextBox(txtConfirm);

            var btnOk = new Button()
            {
                Text = "Change",
                Left = 230,
                Top = 210,
                Width = 90,
                Height = 30,
                DialogResult = DialogResult.OK
            };
            var btnCancel = new Button()
            {
                Text = "Cancel",
                Left = 330,
                Top = 210,
                Width = 80,
                Height = 30,
                DialogResult = DialogResult.Cancel
            };

            UiHelpers.ApplyButtonStyle(btnOk);
            UiHelpers.ApplyButtonStyle(btnCancel);

            dialog.Controls.Add(lblCurrent);
            dialog.Controls.Add(txtCurrent);
            dialog.Controls.Add(lblNew);
            dialog.Controls.Add(txtNew);
            dialog.Controls.Add(lblConfirm);
            dialog.Controls.Add(txtConfirm);
            dialog.Controls.Add(btnOk);
            dialog.Controls.Add(btnCancel);

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            // Validation
            if (string.IsNullOrWhiteSpace(txtCurrent.Text))
            {
                MessageBox.Show("Please enter your current password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNew.Text))
            {
                MessageBox.Show("New password cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNew.Text.Length < 6)
            {
                MessageBox.Show("New password must be at least 6 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNew.Text != txtConfirm.Text)
            {
                MessageBox.Show("New passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                // Verify current password
                var verified = await userService.AuthenticateAsync(_authenticatedUser.UserName, txtCurrent.Text);
                if (verified == null)
                {
                    MessageBox.Show("Current password is incorrect.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update password
                await userService.UpdatePasswordAsync(_authenticatedUser.Id, txtNew.Text);
                MessageBox.Show("Password changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to change password: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _searchCancellation?.Cancel();
                _searchCancellation?.Dispose();
                _searchTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
