using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.UI;

namespace Performance
{
    public partial class UserManagementForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private DataGridView _grid = new DataGridView();
        private UserEntity? _currentUser;

        public UserManagementForm(IUserService userService, UserEntity? currentUser, IServiceProvider? serviceProvider = null)
        {
            _userService = userService;
            _currentUser = currentUser;
            _serviceProvider = serviceProvider!;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Name = "UserManagementForm";
            this.Text = "User Management - Manager Panel";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UiColors.Background;

            // Top panel with title
            var topPanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = UiColors.DarkGreen
            };

            var lblTitle = new Label()
            {
                Text = "USER MANAGEMENT",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Left = 20,
                Top = 18,
                AutoSize = true
            };
            topPanel.Controls.Add(lblTitle);

            // Grid configuration
            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.ReadOnly = true;
            _grid.RowHeadersVisible = false;
            _grid.ColumnHeadersHeight = 40;
            _grid.RowTemplate.Height = 40;

            // Apply UiHelpers styling
            UiHelpers.StyleGrid(_grid);

            // Columns - User ID is hidden
            _grid.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Id",
                HeaderText = "User ID",
                Visible = false  // HIDDEN
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "UserName",
                HeaderText = "Username",
                Width = 300,
                DefaultCellStyle = new DataGridViewCellStyle() { Font = new Font("Segoe UI", 10F, FontStyle.Bold) }
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Role",
                HeaderText = "Role",
                Width = 200,
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Row coloring based on role
            _grid.CellFormatting += Grid_CellFormatting;

            // Bottom panel with buttons
            var bottomPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = UiColors.MediumGreen,
                Padding = new Padding(15)
            };

            var btnAdd = new Button()
            {
                Text = "Add User",
                Left = 15,
                Top = 20,
                Width = 130,
                Height = 35
            };
            var btnEdit = new Button()
            {
                Text = "Edit Role",
                Left = 155,
                Top = 20,
                Width = 130,
                Height = 35
            };
            var btnViewProfile = new Button()
            {
                Text = "View Profile",
                Left = 295,
                Top = 20,
                Width = 140,
                Height = 35
            };
            var btnDelete = new Button()
            {
                Text = "Delete User",
                Left = 445,
                Top = 20,
                Width = 140,
                Height = 35
            };
            var btnRefresh = new Button()
            {
                Text = "Refresh",
                Left = 595,
                Top = 20,
                Width = 110,
                Height = 35
            };

            btnAdd.Click += async (s, e) => await AddUser_Click();
            btnEdit.Click += async (s, e) => await EditRole_Click();
            btnViewProfile.Click += (s, e) => ViewProfile_Click();
            btnDelete.Click += async (s, e) => await DeleteUser_Click();
            btnRefresh.Click += async (s, e) => await RefreshList();

            UiHelpers.ApplyButtonStyle(btnAdd);
            UiHelpers.ApplyButtonStyle(btnEdit);
            UiHelpers.ApplyButtonStyle(btnViewProfile);
            UiHelpers.ApplyButtonStyle(btnDelete);
            UiHelpers.ApplyButtonStyle(btnRefresh);

            bottomPanel.Controls.Add(btnAdd);
            bottomPanel.Controls.Add(btnEdit);
            bottomPanel.Controls.Add(btnViewProfile);
            bottomPanel.Controls.Add(btnDelete);
            bottomPanel.Controls.Add(btnRefresh);

            this.Controls.Add(_grid);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);

            this.Load += async (s, e) => await RefreshList();
            this.ResumeLayout(false);
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Rows[e.RowIndex].DataBoundItem is not UserEntity user) return;

            // Color based on role
            if (_grid.Columns[e.ColumnIndex].HeaderText == "Role")
            {
                e.CellStyle.ForeColor = user.Role switch
                {
                    UserRole.Manager => UiColors.Warning,
                    UserRole.Employee => UiColors.Info,
                    _ => UiColors.PrimaryText
                };
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        private async Task RefreshList()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                _grid.DataSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewProfile_Click()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to view profile.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var user = (UserEntity)_grid.SelectedRows[0].DataBoundItem;

            if (_serviceProvider == null)
            {
                MessageBox.Show("Service provider not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
            
            var form = new UserProfileForm(_userService, taskService, user);
            form.ShowDialog(this);
        }

        private async Task AddUser_Click()
        {
            using var dialog = new Form()
            {
                Text = "Add New User",
                Width = 450,
                Height = 280,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = UiColors.Background,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblUsername = new Label() { Text = "Username:", Left = 20, Top = 20, ForeColor = UiColors.PrimaryText, AutoSize = true };
            var txtUsername = new TextBox() { Left = 20, Top = 45, Width = 390 };
            UiHelpers.StyleTextBox(txtUsername);

            var lblPassword = new Label() { Text = "Password:", Left = 20, Top = 80, ForeColor = UiColors.PrimaryText, AutoSize = true };
            var txtPassword = new TextBox() { Left = 20, Top = 105, Width = 390, UseSystemPasswordChar = true };
            UiHelpers.StyleTextBox(txtPassword);

            var lblRole = new Label() { Text = "Role:", Left = 20, Top = 140, ForeColor = UiColors.PrimaryText, AutoSize = true };
            var cmbRole = new ComboBox() { Left = 20, Top = 165, Width = 390, DropDownStyle = ComboBoxStyle.DropDownList };
            UiHelpers.StyleComboBox(cmbRole);
            cmbRole.Items.AddRange(new object[] { "Manager", "Employee" });
            cmbRole.SelectedIndex = 1;

            var btnOk = new Button() { Text = "Create", Left = 230, Top = 210, Width = 90, Height = 30, DialogResult = DialogResult.OK };
            var btnCancel = new Button() { Text = "Cancel", Left = 330, Top = 210, Width = 80, Height = 30, DialogResult = DialogResult.Cancel };

            UiHelpers.ApplyButtonStyle(btnOk);
            UiHelpers.ApplyButtonStyle(btnCancel);

            dialog.Controls.Add(lblUsername);
            dialog.Controls.Add(txtUsername);
            dialog.Controls.Add(lblPassword);
            dialog.Controls.Add(txtPassword);
            dialog.Controls.Add(lblRole);
            dialog.Controls.Add(cmbRole);
            dialog.Controls.Add(btnOk);
            dialog.Controls.Add(btnCancel);

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Username and password are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var role = cmbRole.SelectedItem.ToString() == "Manager" ? UserRole.Manager : UserRole.Employee;
                await _userService.CreateAsync(txtUsername.Text.Trim(), txtPassword.Text, role);
                MessageBox.Show($"User '{txtUsername.Text}' created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task EditRole_Click()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var user = (UserEntity)_grid.SelectedRows[0].DataBoundItem;

            using var dialog = new Form()
            {
                Text = $"Edit Role - {user.UserName}",
                Width = 400,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = UiColors.Background,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblRole = new Label() { Text = "Select New Role:", Left = 20, Top = 20, ForeColor = UiColors.PrimaryText, AutoSize = true };
            var cmbRole = new ComboBox() { Left = 20, Top = 45, Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
            UiHelpers.StyleComboBox(cmbRole);
            cmbRole.Items.AddRange(new object[] { "Manager", "Employee" });
            cmbRole.SelectedIndex = user.Role == UserRole.Manager ? 0 : 1;

            var btnOk = new Button() { Text = "Update", Left = 190, Top = 120, Width = 80, Height = 30, DialogResult = DialogResult.OK };
            var btnCancel = new Button() { Text = "Cancel", Left = 280, Top = 120, Width = 80, Height = 30, DialogResult = DialogResult.Cancel };

            UiHelpers.ApplyButtonStyle(btnOk);
            UiHelpers.ApplyButtonStyle(btnCancel);

            dialog.Controls.Add(lblRole);
            dialog.Controls.Add(cmbRole);
            dialog.Controls.Add(btnOk);
            dialog.Controls.Add(btnCancel);

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                user.Role = cmbRole.SelectedItem.ToString() == "Manager" ? UserRole.Manager : UserRole.Employee;
                await _userService.UpdateAsync(user);
                MessageBox.Show($"Role updated successfully for '{user.UserName}'!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update role: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteUser_Click()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var user = (UserEntity)_grid.SelectedRows[0].DataBoundItem;

            // Prevent deleting current user
            if (_currentUser != null && user.Id == _currentUser.Id)
            {
                MessageBox.Show("You cannot delete your own account!", "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{user.UserName}'?\n\nThis action cannot be undone.",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                await _userService.DeleteAsync(user.Id);
                MessageBox.Show($"User '{user.UserName}' deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
