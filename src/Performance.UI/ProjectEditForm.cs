using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private readonly IProjectSuggestionService? _projectSuggestionService;
        private readonly UserEntity? _currentUser;
        private ProjectEntity? _project;

        private TextBox txtName = new TextBox();
        private TextBox txtDesc = new TextBox();
        private TextBox txtManagerNotes = new TextBox();

        public ProjectEditForm(
            IProjectService projectService,
            IProjectSuggestionService projectSuggestionService,
            UserEntity? currentUser = null)
        {
            _projectService = projectService;
            _projectSuggestionService = projectSuggestionService;
            _currentUser = currentUser;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            bool isManager = _currentUser?.Role == UserRole.Manager;
            int height = isManager ? 900 : 800;
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
                BackColor = UiColors.Background,
                AutoScroll = true
            };

            // Project Name
            var lblName = new Label()
            {
                Text = "Project Name:",
                Left = 0,
                Top = 10,
                Width = 640,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            txtName = new TextBox()
            {
                Left = 0,
                Top = 35,
                Width = 640,
                Font = new Font("Segoe UI", 10F)
            };
            UiHelpers.StyleTextBox(txtName);

            // Description
            var lblDesc = new Label()
            {
                Text = "Description:",
                Left = 0,
                Top = 75,
                Width = 640,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            txtDesc = new TextBox()
            {
                Left = 0,
                Top = 100,
                Width = 640,
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

            // ??????????????????????????????????????????????????
            // ?   AI PANEL - NO BORDERS, ONLY COLORED TITLES   ?
            // ??????????????????????????????????????????????????
            if (_projectSuggestionService != null)
            {
                var aiPanel = new Panel()
                {
                    Left = 0,
                    Top = currentTop,
                    Width = 660,
                    Height = 500,
                    BackColor = Color.FromArgb(250, 250, 250),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Gradient Header
                var headerPanel = new Panel()
                {
                    Left = 0,
                    Top = 0,
                    Width = 660,
                    Height = 50
                };

                headerPanel.Paint += (s, e) =>
                {
                    using (var brush = new LinearGradientBrush(
                        headerPanel.ClientRectangle,
                        Color.FromArgb(67, 160, 71),
                        Color.FromArgb(27, 94, 32),
                        LinearGradientMode.Horizontal))
                    {
                        e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
                    }
                };

                var lblAiTitle = new Label()
                {
                    Text = "*** AI PROJECT INSIGHTS ***",
                    Left = 15,
                    Top = 15,
                    Width = 400,
                    Height = 24,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };

                var btnGetSuggestions = new Button()
                {
                    Text = "Analyze your project",
                    Left = 500,
                    Top = 10,
                    Width = 145,
                    Height = 32,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(67, 160, 71),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnGetSuggestions.FlatAppearance.BorderSize = 0;

                headerPanel.Controls.Add(lblAiTitle);
                headerPanel.Controls.Add(btnGetSuggestions);

                // Progress Bar
                var progressBar = new ProgressBar()
                {
                    Left = 0,
                    Top = 50,
                    Width = 660,
                    Height = 8,
                    Style = ProgressBarStyle.Marquee,
                    MarqueeAnimationSpeed = 30,
                    Visible = false
                };

                // SCROLLABLE RESULTS - NO BORDERS!
                var resultsPanel = new Panel()
                {
                    Left = 0,
                    Top = 58,
                    Width = 660,
                    Height = 442,
                    AutoScroll = true,
                    BackColor = Color.White
                };

                // Helper to create section (NO BOX, just colored title + content)
                Label CreateSection(string icon, string title, Color color, int top)
                {
                    var lblTitle = new Label()
                    {
                        Text = $"{icon} {title}",
                        Left = 15,
                        Top = top,
                        Width = 620,
                        Height = 25,
                        Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                        ForeColor = color,
                        BackColor = Color.Transparent
                    };

                    var lblContent = new Label()
                    {
                        Text = "",
                        Left = 15,
                        Top = top + 28,
                        Width = 620,
                        Height = 60,
                        Font = new Font("Segoe UI", 9F),
                        ForeColor = Color.FromArgb(60, 60, 60),
                        BackColor = Color.Transparent,
                        AutoSize = false
                    };

                    resultsPanel.Controls.Add(lblTitle);
                    resultsPanel.Controls.Add(lblContent);

                    return lblContent;
                }

                var lblFeatures = CreateSection("[*]", "SUGGESTED FEATURES", Color.FromArgb(33, 150, 243), 15);
                var lblTasks = CreateSection("[+]", "RECOMMENDED TASKS", Color.FromArgb(76, 175, 80), 110);
                var lblRoles = CreateSection("[#]", "REQUIRED ROLES", Color.FromArgb(255, 152, 0), 205);
                var lblTeam = CreateSection("[@]", "TEAM COMPOSITION", Color.FromArgb(156, 39, 176), 300);
                var lblInsights = CreateSection("[!]", "AI INSIGHTS", Color.FromArgb(0, 150, 136), 395);

                // Empty state
                var lblEmptyState = new Label()
                {
                    Text = "Click 'Get Suggestions' to analyze your project with AI\n\nEnter a project name and description first.",
                    Left = 50,
                    Top = 200,
                    Width = 560,
                    Height = 60,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    BackColor = Color.Transparent
                };
                resultsPanel.Controls.Add(lblEmptyState);

                aiPanel.Controls.Add(headerPanel);
                aiPanel.Controls.Add(progressBar);
                aiPanel.Controls.Add(resultsPanel);

                // Button Click Event
                btnGetSuggestions.Click += async (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        MessageBox.Show("Please enter a project name first.", "Info",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtName.Focus();
                        return;
                    }

                    btnGetSuggestions.Enabled = false;
                    btnGetSuggestions.Text = "Analyzing...";
                    progressBar.Visible = true;
                    lblEmptyState.Text = " AI is analyzing...\n\nPlease wait...";
                    lblEmptyState.ForeColor = Color.FromArgb(67, 160, 71);

                    // Hide all content labels
                    lblFeatures.Visible = false;
                    lblTasks.Visible = false;
                    lblRoles.Visible = false;
                    lblTeam.Visible = false;
                    lblInsights.Visible = false;

                    try
                    {
                        var result = await _projectSuggestionService.SuggestProjectDetailsAsync(
                            txtName.Text,
                            txtDesc.Text
                        );

                        // Hide empty state
                        lblEmptyState.Visible = false;

                        // Update and show content
                        lblFeatures.Text = result.SuggestedFeatures;
                        lblFeatures.Visible = true;

                        lblTasks.Text = result.RecommendedTasks;
                        lblTasks.Visible = true;

                        lblRoles.Text = result.RequiredEmployeeTypes;
                        lblRoles.Visible = true;

                        lblTeam.Text = result.TeamComposition;
                        lblTeam.Visible = true;

                        lblInsights.Text = result.Explanation.Replace(" Gemini AI Analysis: ", "");
                        lblInsights.Height = 100;  // More space for insights
                        lblInsights.Visible = true;

                        // Scroll to top
                        resultsPanel.AutoScrollPosition = new Point(0, 0);
                    }
                    catch (Exception ex)
                    {
                        lblEmptyState.Visible = true;
                        lblEmptyState.Text = $" Error: {ex.Message}\n\nCheck API key & connection";
                        lblEmptyState.ForeColor = Color.FromArgb(211, 47, 47);
                    }
                    finally
                    {
                        btnGetSuggestions.Enabled = true;
                        btnGetSuggestions.Text = "Analyze your project";
                        progressBar.Visible = false;
                    }
                };

                mainPanel.Controls.Add(aiPanel);
                currentTop += 510;
            }

            // Manager Notes
            if (isManager)
            {
                var lblNotes = new Label()
                {
                    Text = "Manager Notes:",
                    Left = 0,
                    Top = currentTop,
                    Width = 640,
                    ForeColor = UiColors.PrimaryText,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };

                txtManagerNotes = new TextBox()
                {
                    Left = 0,
                    Top = currentTop + 25,
                    Width = 640,
                    Height = 80,
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
                Left = 420,
                Top = 12
            };
            var btnCancel = new Button()
            {
                Text = "Cancel",
                Width = 100,
                Height = 35,
                Left = 530,
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
