using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.UI;
using TaskStatus = Performance.Domain.Enums.TaskStatus;

namespace Performance
{
    public partial class UserProfileForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly IUserService _userService;
        private readonly ITaskService _taskService;
        private UserEntity _user;
        
        private TextBox _txtUsername = null!;
        private TextBox _txtEmail = null!;
        private TextBox _txtFullName = null!;
        private TextBox _txtSector = null!;
        private PictureBox _picProfile = null!;
        private DataGridView _gridMyTasks = null!;

        public UserProfileForm(IUserService userService, ITaskService taskService, UserEntity user)
        {
            _userService = userService;
            _taskService = taskService;
            _user = user;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(900, 740);
            this.Name = "UserProfileForm";
            this.Text = "My Profile";
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
                Text = "MY PROFILE",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Left = 20,
                Top = 18,
                AutoSize = true
            };
            topPanel.Controls.Add(lblTitle);

            // Main content panel
            var contentPanel = new Panel()
            {
                Left = 20,
                Top = 80,
                Width = 860,
                Height = 290,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Profile Photo
            _picProfile = new PictureBox()
            {
                Left = 20,
                Top = 20,
                Width = 150,
                Height = 150,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = UiColors.Background
            };

            var btnUploadPhoto = new Button()
            {
                Text = "Upload Photo",
                Left = 20,
                Top = 180,
                Width = 150,
                Height = 30
            };
            UiHelpers.ApplyButtonStyle(btnUploadPhoto);
            btnUploadPhoto.Click += BtnUploadPhoto_Click;

            // Profile fields
            var lblUsername = new Label()
            {
                Text = "Username:",
                Left = 200,
                Top = 25,
                Width = 120,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            _txtUsername = new TextBox()
            {
                Left = 330,
                Top = 22,
                Width = 500,
                ReadOnly = true,
                BackColor = UiColors.Lighten(UiColors.Background, 0.5f)
            };
            UiHelpers.StyleTextBox(_txtUsername);

            var lblEmail = new Label()
            {
                Text = "Email:",
                Left = 200,
                Top = 70,
                Width = 120,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            _txtEmail = new TextBox()
            {
                Left = 330,
                Top = 67,
                Width = 500
            };
            UiHelpers.StyleTextBox(_txtEmail);

            var lblFullName = new Label()
            {
                Text = "Full Name:",
                Left = 200,
                Top = 115,
                Width = 120,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            _txtFullName = new TextBox()
            {
                Left = 330,
                Top = 112,
                Width = 500
            };
            UiHelpers.StyleTextBox(_txtFullName);

            var lblSector = new Label()
            {
                Text = "Sector/Field:",
                Left = 200,
                Top = 160,
                Width = 120,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            _txtSector = new TextBox()
            {
                Left = 330,
                Top = 157,
                Width = 500
            };
            UiHelpers.StyleTextBox(_txtSector);

            var lblRole = new Label()
            {
                Text = "Role:",
                Left = 200,
                Top = 205,
                Width = 120,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var txtRole = new TextBox()
            {
                Left = 330,
                Top = 202,
                Width = 200,
                ReadOnly = true,
                BackColor = UiColors.Lighten(UiColors.Background, 0.5f)
            };
            UiHelpers.StyleTextBox(txtRole);

            contentPanel.Controls.Add(_picProfile);
            contentPanel.Controls.Add(btnUploadPhoto);
            contentPanel.Controls.Add(lblUsername);
            contentPanel.Controls.Add(_txtUsername);
            contentPanel.Controls.Add(lblEmail);
            contentPanel.Controls.Add(_txtEmail);
            contentPanel.Controls.Add(lblFullName);
            contentPanel.Controls.Add(_txtFullName);
            contentPanel.Controls.Add(lblSector);
            contentPanel.Controls.Add(_txtSector);
            contentPanel.Controls.Add(lblRole);
            contentPanel.Controls.Add(txtRole);

            // My Tasks section
            var lblMyTasks = new Label()
            {
                Text = "MY TASKS",
                Left = 20,
                Top = 390,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = UiColors.PrimaryText,
                AutoSize = true
            };

            _gridMyTasks = new DataGridView()
            {
                Left = 20,
                Top = 420,
                Width = 860,
                Height = 200,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                ColumnHeadersHeight = 35,
                RowTemplate = { Height = 30 }
            };

            UiHelpers.StyleGrid(_gridMyTasks);

            // Columns for My Tasks grid
            _gridMyTasks.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Title",
                HeaderText = "Task",
                Width = 300,
                DefaultCellStyle = new DataGridViewCellStyle() { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _gridMyTasks.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Status",
                HeaderText = "Status",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _gridMyTasks.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Priority",
                HeaderText = "Priority",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _gridMyTasks.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Deadline",
                HeaderText = "Deadline",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle() { Format = "yyyy-MM-dd" }
            });

            _gridMyTasks.CellFormatting += GridMyTasks_CellFormatting;

            // Bottom panel with buttons
            var bottomPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = UiColors.MediumGreen
            };

            var btnSave = new Button()
            {
                Text = "Save Changes",
                Left = 620,
                Top = 12,
                Width = 130,
                Height = 35
            };

            var btnClose = new Button()
            {
                Text = "Close",
                Left = 760,
                Top = 12,
                Width = 120,
                Height = 35
            };

            btnSave.Click += async (s, e) => await SaveProfile_Click();
            btnClose.Click += (s, e) => this.Close();

            UiHelpers.ApplyButtonStyle(btnSave);
            UiHelpers.ApplyButtonStyle(btnClose);

            bottomPanel.Controls.Add(btnSave);
            bottomPanel.Controls.Add(btnClose);

            this.Controls.Add(topPanel);
            this.Controls.Add(contentPanel);
            this.Controls.Add(lblMyTasks);
            this.Controls.Add(_gridMyTasks);
            this.Controls.Add(bottomPanel);

            // Load data
            _txtUsername.Text = _user.UserName;
            _txtEmail.Text = _user.Email ?? string.Empty;
            _txtFullName.Text = _user.FullName ?? string.Empty;
            _txtSector.Text = _user.Sector ?? string.Empty;
            txtRole.Text = _user.Role.ToString();

            // Load profile photo
            LoadProfilePhoto();

            this.Load += async (s, e) => await LoadMyTasks();
            this.ResumeLayout(false);
        }

        private void LoadProfilePhoto()
        {
            if (!string.IsNullOrWhiteSpace(_user.ProfilePhoto))
            {
                try
                {
                    // Try to load from base64
                    if (_user.ProfilePhoto.StartsWith("data:image"))
                    {
                        var base64Data = _user.ProfilePhoto.Split(',')[1];
                        var imageBytes = Convert.FromBase64String(base64Data);
                        using var ms = new MemoryStream(imageBytes);
                        _picProfile.Image = Image.FromStream(ms);
                    }
                    else if (File.Exists(_user.ProfilePhoto))
                    {
                        _picProfile.Image = Image.FromFile(_user.ProfilePhoto);
                    }
                }
                catch
                {
                    // If loading fails, show default
                    _picProfile.Image = null;
                }
            }
        }

        private void BtnUploadPhoto_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog()
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select Profile Photo"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var image = Image.FromFile(openFileDialog.FileName);
                    _picProfile.Image = image;

                    // Convert to base64
                    using var ms = new MemoryStream();
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    var base64 = Convert.ToBase64String(ms.ToArray());
                    _user.ProfilePhoto = $"data:image/jpeg;base64,{base64}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task LoadMyTasks()
        {
            try
            {
                // Get all tasks assigned to current user
                var allTasks = await _taskService.ListAsync();
                var myTasks = allTasks.Where(t => t.AssignedToUserId == _user.Id.ToString()).ToList();
                
                _gridMyTasks.DataSource = myTasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load tasks: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GridMyTasks_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_gridMyTasks.Rows[e.RowIndex].DataBoundItem is not TaskEntity task) return;

            // Color based on status
            if (_gridMyTasks.Columns[e.ColumnIndex].HeaderText == "Status")
            {
                e.CellStyle.ForeColor = task.Status switch
                {
                    TaskStatus.Done => UiColors.StatusDone,
                    TaskStatus.InProgress => UiColors.StatusInProgress,
                    TaskStatus.Todo => UiColors.StatusTodo,
                    _ => UiColors.PrimaryText
                };
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            // Color based on priority
            if (_gridMyTasks.Columns[e.ColumnIndex].HeaderText == "Priority")
            {
                e.CellStyle.ForeColor = task.Priority switch
                {
                    TaskPriority.High => UiColors.PriorityHigh,
                    TaskPriority.Medium => UiColors.PriorityMedium,
                    TaskPriority.Low => UiColors.PriorityLow,
                    _ => UiColors.PrimaryText
                };
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        private async Task SaveProfile_Click()
        {
            try
            {
                // Validate email
                if (!string.IsNullOrWhiteSpace(_txtEmail.Text))
                {
                    if (!_txtEmail.Text.Contains("@"))
                    {
                        MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Update user profile
                _user.Email = _txtEmail.Text.Trim();
                _user.FullName = _txtFullName.Text.Trim();
                _user.Sector = _txtSector.Text.Trim();

                await _userService.UpdateAsync(_user);
                
                MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update profile: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
