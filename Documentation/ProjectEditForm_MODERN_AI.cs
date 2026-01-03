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
            int height = isManager ? 820 : 720;  // ? Increased for modern AI panel
            this.ClientSize = new System.Drawing.Size(720, height);
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
                Width = 660,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            
            txtName = new TextBox() 
            { 
                Left = 0, 
                Top = 35, 
                Width = 660,
                Font = new Font("Segoe UI", 10F)
            };
            UiHelpers.StyleTextBox(txtName);

            // Description
            var lblDesc = new Label() 
            { 
                Text = "Description:", 
                Left = 0, 
                Top = 75, 
                Width = 660,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            
            txtDesc = new TextBox() 
            { 
                Left = 0, 
                Top = 100, 
                Width = 660, 
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
            // ?  ?? MODERN AI PROJECT INSIGHTS PANEL          ?
            // ??????????????????????????????????????????????????
            if (_projectSuggestionService != null)
            {
                var aiContainer = new Panel()
                {
                    Left = 0,
                    Top = currentTop,
                    Width = 660,
                    Height = 360,
                    BackColor = Color.FromArgb(245, 245, 245),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // ? HEADER
                var aiHeader = new Panel()
                {
                    Left = 0,
                    Top = 0,
                    Width = 660,
                    Height = 45,
                    BackColor = Color.FromArgb(67, 160, 71)  // Green
                };

                var lblAiHeader = new Label()
                {
                    Text = "?? AI PROJECT INSIGHTS",
                    Left = 15,
                    Top = 12,
                    Width = 400,
                    Height = 24,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White
                };

                var btnGetSuggestions = new Button()
                {
                    Text = "Get Suggestions",
                    Left = 500,
                    Top = 8,
                    Width = 145,
                    Height = 30,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(67, 160, 71),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnGetSuggestions.FlatAppearance.BorderSize = 0;

                aiHeader.Controls.Add(lblAiHeader);
                aiHeader.Controls.Add(btnGetSuggestions);

                // ? PROGRESS BAR
                var progressAi = new ProgressBar()
                {
                    Left = 0,
                    Top = 45,
                    Width = 660,
                    Height = 6,
                    Style = ProgressBarStyle.Marquee,
                    MarqueeAnimationSpeed = 30,
                    Visible = false
                };

                // ? RESULTS PANEL
                var resultsPanel = new Panel()
                {
                    Left = 10,
                    Top = 55,
                    Width = 640,
                    Height = 295,
                    AutoScroll = true,
                    BackColor = Color.FromArgb(250, 250, 250)
                };

                // Helper function to create result boxes
                Panel CreateResultBox(string icon, string title, Color accentColor, int top, int height = 50)
                {
                    var box = new Panel()
                    {
                        Left = 5,
                        Top = top,
                        Width = 610,
                        Height = height,
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Visible = false  // Hidden initially
                    };

                    var accent = new Panel()
                    {
                        Left = 0,
                        Top = 0,
                        Width = 4,
                        Height = height,
                        BackColor = accentColor
                    };

                    var lblIcon = new Label()
                    {
                        Text = icon,
                        Left = 10,
                        Top = height / 2 - 10,
                        Width = 30,
                        Height = 20,
                        Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                        ForeColor = accentColor
                    };

                    var lblTitle = new Label()
                    {
                        Text = title,
                        Left = 45,
                        Top = 5,
                        Width = 550,
                        Height = 18,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(60, 60, 60)
                    };

                    var lblContent = new Label()
                    {
                        Text = "",
                        Left = 45,
                        Top = 25,
                        Width = 550,
                        Height = height - 30,
                        Font = new Font("Segoe UI", 8.5F),
                        ForeColor = Color.FromArgb(90, 90, 90)
                    };

                    box.Controls.Add(accent);
                    box.Controls.Add(lblIcon);
                    box.Controls.Add(lblTitle);
                    box.Controls.Add(lblContent);
                    box.Tag = lblContent;  // Store for later update

                    return box;
                }

                var boxFeatures = CreateResultBox("??", "SUGGESTED FEATURES", Color.FromArgb(33, 150, 243), 5, 60);
                var boxTasks = CreateResultBox("?", "RECOMMENDED TASKS", Color.FromArgb(76, 175, 80), 70, 60);
                var boxRoles = CreateResultBox("??", "REQUIRED ROLES", Color.FromArgb(255, 152, 0), 135, 50);
                var boxTeam = CreateResultBox("??", "TEAM COMPOSITION", Color.FromArgb(156, 39, 176), 190, 50);
                var boxInsights = CreateResultBox("??", "AI INSIGHTS", Color.FromArgb(0, 150, 136), 245, 60);

                resultsPanel.Controls.Add(boxFeatures);
                resultsPanel.Controls.Add(boxTasks);
                resultsPanel.Controls.Add(boxRoles);
                resultsPanel.Controls.Add(boxTeam);
                resultsPanel.Controls.Add(boxInsights);

                // Empty state message
                var lblEmptyState = new Label()
                {
                    Text = "Click 'Get Suggestions' to analyze your project with AI\n\nEnter a project name and description first for better results.",
                    Left = 20,
                    Top = 120,
                    Width = 600,
                    Height = 60,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                    ForeColor = Color.Gray
                };
                resultsPanel.Controls.Add(lblEmptyState);

                aiContainer.Controls.Add(aiHeader);
                aiContainer.Controls.Add(progressAi);
                aiContainer.Controls.Add(resultsPanel);

                // ? BUTTON CLICK EVENT
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
                    progressAi.Visible = true;
                    lblEmptyState.Text = "? AI is analyzing your project...\n\nPlease wait, this may take a few seconds.";
                    lblEmptyState.ForeColor = Color.FromArgb(67, 160, 71);

                    // Hide all result boxes
                    boxFeatures.Visible = false;
                    boxTasks.Visible = false;
                    boxRoles.Visible = false;
                    boxTeam.Visible = false;
                    boxInsights.Visible = false;

                    try
                    {
                        var result = await _projectSuggestionService.SuggestProjectDetailsAsync(
                            txtName.Text,
                            txtDesc.Text
                        );

                        // Hide empty state
                        lblEmptyState.Visible = false;

                        // Update and show boxes
                        ((Label)boxFeatures.Tag!).Text = result.SuggestedFeatures;
                        boxFeatures.Visible = true;

                        ((Label)boxTasks.Tag!).Text = result.RecommendedTasks;
                        boxTasks.Visible = true;

                        ((Label)boxRoles.Tag!).Text = result.RequiredEmployeeTypes;
                        boxRoles.Visible = true;

                        ((Label)boxTeam.Tag!).Text = result.TeamComposition;
                        boxTeam.Visible = true;

                        ((Label)boxInsights.Tag!).Text = result.Explanation.Replace("?? Gemini AI Analysis: ", "");
                        boxInsights.Visible = true;
                    }
                    catch (Exception ex)
                    {
                        lblEmptyState.Visible = true;
                        lblEmptyState.Text = $"? Error: {ex.Message}\n\nPlease check:\n• Gemini API key is configured in appsettings.json\n• Internet connection is active";
                        lblEmptyState.ForeColor = Color.FromArgb(244, 67, 54);
                    }
                    finally
                    {
                        btnGetSuggestions.Enabled = true;
                        btnGetSuggestions.Text = "Get Suggestions";
                        progressAi.Visible = false;
                    }
                };

                mainPanel.Controls.Add(aiContainer);
                currentTop += 370;
            }

            // ??????????????????????????????????????????????????
            // ?  ?? MANAGER NOTES (ONLY FOR MANAGERS)        ?
            // ??????????????????????????????????????????????????
            if (isManager)
            {
                var lblNotes = new Label() 
                { 
                    Text = "Manager Notes:", 
                    Left = 0, 
                    Top = currentTop, 
                    Width = 660,
                    ForeColor = UiColors.PrimaryText,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                
                txtManagerNotes = new TextBox() 
                { 
                    Left = 0, 
                    Top = currentTop + 25, 
                    Width = 660, 
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
                Left = 480,
                Top = 12
            };
            var btnCancel = new Button() 
            { 
                Text = "Cancel", 
                Width = 100, 
                Height = 35,
                Left = 590,
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
