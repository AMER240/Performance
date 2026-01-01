using System;
using System.Drawing;
using System.Windows.Forms;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.UI;

namespace Performance
{
    public partial class ProjectEditForm : BaseForm
    {
        private readonly IProjectService _projectService;
        private readonly UserEntity? _currentUser;
        private ProjectEntity? _project;

        private TextBox txtName = new TextBox();
        private TextBox txtDesc = new TextBox();
        private TextBox txtManagerNotes = new TextBox();

        public ProjectEditForm(IProjectService projectService, UserEntity? currentUser = null)
        {
            _projectService = projectService;
            _currentUser = currentUser;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            bool isManager = _currentUser?.Role == UserRole.Manager;
            int height = isManager ? 450 : 350;
            this.ClientSize = new System.Drawing.Size(700, height);
            this.Name = "ProjectEditForm";
            this.Text = "Project Editor";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UiColors.Background;

            // Title panel
            var titlePanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = UiColors.DarkGreen
            };

            var lblFormTitle = new Label()
            {
                Text = "PROJECT DETAILS",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Left = 20,
                Top = 14,
                AutoSize = true
            };
            titlePanel.Controls.Add(lblFormTitle);

            // Main container
            var mainPanel = new Panel()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = UiColors.Background
            };

            var lblName = new Label() 
            { 
                Text = "Project Name:", 
                Left = 0, 
                Top = 10, 
                Width = 560,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            
            txtName = new TextBox() 
            { 
                Left = 0, 
                Top = 35, 
                Width = 560,
                Font = new Font("Segoe UI", 10F)
            };
            UiHelpers.StyleTextBox(txtName);

            var lblDesc = new Label() 
            { 
                Text = "Description:", 
                Left = 0, 
                Top = 75, 
                Width = 560,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            
            txtDesc = new TextBox() 
            { 
                Left = 0, 
                Top = 100, 
                Width = 560, 
                Height = 80,
                Multiline = true,
                Font = new Font("Segoe UI", 10F)
            };
            UiHelpers.StyleTextBox(txtDesc);

            mainPanel.Controls.Add(lblName);
            mainPanel.Controls.Add(txtName);
            mainPanel.Controls.Add(lblDesc);
            mainPanel.Controls.Add(txtDesc);

            int currentTop = 190;

            if (isManager)
            {
                var lblNotes = new Label() 
                { 
                    Text = "Manager Notes:", 
                    Left = 0, 
                    Top = currentTop, 
                    Width = 560,
                    ForeColor = UiColors.PrimaryText,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                
                txtManagerNotes = new TextBox() 
                { 
                    Left = 0, 
                    Top = currentTop + 25, 
                    Width = 560, 
                    Height = 60,
                    Multiline = true,
                    Font = new Font("Segoe UI", 10F)
                };
                UiHelpers.StyleTextBox(txtManagerNotes);

                mainPanel.Controls.Add(lblNotes);
                mainPanel.Controls.Add(txtManagerNotes);
            }

            // Bottom panel with buttons
            var bottomPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = UiColors.MediumGreen
            };

            var btnSave = new Button() 
            { 
                Text = "Save", 
                Width = 100, 
                Height = 35,
                Left = 370,
                Top = 12
            };
            var btnCancel = new Button() 
            { 
                Text = "Cancel", 
                Width = 100, 
                Height = 35,
                Left = 480,
                Top = 12
            };

            btnSave.Click += async (s, e) => await Save_Click();
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            
            UiHelpers.ApplyButtonStyle(btnSave);
            UiHelpers.ApplyButtonStyle(btnCancel);

            bottomPanel.Controls.Add(btnSave);
            bottomPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);
            this.Controls.Add(titlePanel);
            this.Controls.Add(bottomPanel);
            
            this.ResumeLayout(false);
        }

        public void LoadProject(ProjectEntity project)
        {
            _project = project;
            txtName.Text = project.Name;
            txtDesc.Text = project.Description ?? string.Empty;
            if (_currentUser?.Role == UserRole.Manager && txtManagerNotes != null)
            {
                txtManagerNotes.Text = project.ManagerNotes ?? string.Empty;
            }
        }

        private async System.Threading.Tasks.Task Save_Click()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a project name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                if (_project == null)
                {
                    var p = new ProjectEntity 
                    { 
                        Name = txtName.Text.Trim(), 
                        Description = txtDesc.Text.Trim(), 
                        CreatedAt = DateTime.UtcNow,
                        ManagerNotes = _currentUser?.Role == UserRole.Manager ? txtManagerNotes?.Text?.Trim() : null
                    };
                    await _projectService.CreateAsync(p);
                }
                else
                {
                    _project.Name = txtName.Text.Trim();
                    _project.Description = txtDesc.Text.Trim();
                    if (_currentUser?.Role == UserRole.Manager && txtManagerNotes != null)
                    {
                        _project.ManagerNotes = txtManagerNotes.Text.Trim();
                    }
                    await _projectService.UpdateAsync(_project);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save project: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
