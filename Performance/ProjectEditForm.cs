using System;
using System.Drawing;
using System.Windows.Forms;
using Performance.Application.Services;
using Performance.Infrastructure.Entities;
using Performance.UI;

namespace Performance
{
    public partial class ProjectEditForm : BaseForm
    {
        private readonly IProjectService _projectService;
        private readonly IProjectSuggestionService _projectSuggestionService;
        private readonly UserEntity? _currentUser;
        private ProjectEntity? _project;

        private TextBox txtName = new TextBox();
        private TextBox txtDesc = new TextBox();
        private TextBox txtManagerNotes = new TextBox();

        public ProjectEditForm(IProjectService projectService, IProjectSuggestionService projectSuggestionService, UserEntity? currentUser = null)
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
            int height = isManager ? 850 : 750;  // Increased for modern AI panel
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


            // ????????????????????????????????????????????????????????????????
            // MODERN AI PROJECT INSIGHTS PANEL
            // Professional UI with color-coded categories and smooth UX
            // ????????????????????????????????????????????????????????????????

            // Main AI Panel Container
            var aiPanel = new Panel()
            {
                Left = 0,
                Top = 190,
                Width = 660,
                Height = 450,
                BackColor = Color.FromArgb(245, 245, 245), // Light gray background
                BorderStyle = BorderStyle.FixedSingle
            };

            // ??????????????????????????????????????????
            // HEADER PANEL with Green Theme
            // ??????????????????????????????????????????
            var headerPanel = new Panel()
            {
                Left = 0,
                Top = 0,
                Width = 660,
                Height = 50,
                BackColor = Color.FromArgb(67, 160, 71) // Green
            };

            var lblAiTitle = new Label()
            {
                Text = "AI PROJECT INSIGHTS",
                Left = 15,
                Top = 12,
                Width = 300,
                Height = 25,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            var btnGetSuggestions = new Button()
            {
                Text = "Analyze with AI",
                Left = 520,
                Top = 10,
                Width = 130,
                Height = 32,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(67, 160, 71),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGetSuggestions.FlatAppearance.BorderSize = 0;

            headerPanel.Controls.Add(lblAiTitle);
            headerPanel.Controls.Add(btnGetSuggestions);

            // ??????????????????????????????????????????
            // PROGRESS BAR
            // ??????????????????????????????????????????
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

            // ??????????????????????????????????????????
            // RESULTS CONTAINER (Scrollable)
            // ??????????????????????????????????????????
            var resultsContainer = new Panel()
            {
                Left = 0,
                Top = 58,
                Width = 660,
                Height = 392,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            // ??????????????????????????????????????????
            // LOADING / EMPTY STATE LABEL
            // ??????????????????????????????????????????
            var lblEmptyState = new Label()
            {
                Text = "Click 'Get Suggestions' to analyze your project with AI",
                Left = 20,
                Top = 150,
                Width = 620,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            resultsContainer.Controls.Add(lblEmptyState);

            // ??????????????????????????????????????????
            // HELPER METHOD: Create Result Box
            // ??????????????????????????????????????????
            Panel CreateResultBox(string title, string content, Color themeColor, int yPosition)
            {
                var box = new Panel()
                {
                    Left = 10,
                    Top = yPosition,
                    Width = 620,
                    Height = 0, // Will auto-size
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(10)
                };

                // Title with icon
                var lblTitle = new Label()
                {
                    Text = title,
                    Left = 5,
                    Top = 5,
                    Width = 600,
                    Height = 25,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = themeColor,
                    BackColor = Color.Transparent
                };

                // Separator line
                var separator = new Panel()
                {
                    Left = 5,
                    Top = 30,
                    Width = 600,
                    Height = 2,
                    BackColor = themeColor
                };

                // Content text with word wrap
                var lblContent = new Label()
                {
                    Text = content,
                    Left = 5,
                    Top = 40,
                    Width = 600,
                    AutoSize = true,
                    MaximumSize = new Size(600, 0),
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = Color.FromArgb(60, 60, 60)
                };

                box.Controls.Add(lblTitle);
                box.Controls.Add(separator);
                box.Controls.Add(lblContent);

                // Auto-size the box based on content
                box.Height = lblContent.Bottom + 15;

                return box;
            }

            // ??????????????????????????????????????????
            // HELPER: Format list items with bullets
            // ??????????????????????????????????????????
            string FormatListItems(string text)
            {
                if (string.IsNullOrWhiteSpace(text)) return text;

                var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var formatted = new System.Text.StringBuilder();

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Remove existing bullets/numbers
                    line = System.Text.RegularExpressions.Regex.Replace(line, @"^[\d\.\-\*\•]+\s*", "");

                    // Add bullet point
                    formatted.AppendLine($"  • {line}");
                }

                return formatted.ToString();
            }

            // ??????????????????????????????????????????
            // BUTTON CLICK EVENT
            // ??????????????????????????????????????????
            btnGetSuggestions.Click += async (s, e) =>
            {
                // Clear previous results
                resultsContainer.Controls.Clear();

                // Show loading state
                btnGetSuggestions.Enabled = false;
                btnGetSuggestions.Text = "Analyzing...";
                progressBar.Visible = true;

                var lblLoading = new Label()
                {
                    Text = "AI is analyzing your project...\nThis may take a few seconds.",
                    Left = 20,
                    Top = 150,
                    Width = 620,
                    Height = 80,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = Color.FromArgb(67, 160, 71)
                };
                resultsContainer.Controls.Add(lblLoading);

                try
                {
                    // Call AI API
                    var result = await _projectSuggestionService.SuggestProjectDetailsAsync(
                        txtName.Text,
                        txtDesc.Text
                    );

                    // Clear loading message
                    resultsContainer.Controls.Clear();

                    // Define colors for each category
                    var featuresColor = Color.FromArgb(33, 150, 243);   // Blue
                    var tasksColor = Color.FromArgb(76, 175, 80);       // Green
                    var rolesColor = Color.FromArgb(255, 152, 0);       // Orange
                    var teamColor = Color.FromArgb(156, 39, 176);       // Purple
                    var insightsColor = Color.FromArgb(0, 150, 136);    // Teal

                    int currentY = 10;

                    // FEATURES
                    if (!string.IsNullOrWhiteSpace(result.SuggestedFeatures))
                    {
                        var featuresBox = CreateResultBox(
                            "[*] SUGGESTED FEATURES",
                            FormatListItems(result.SuggestedFeatures),
                            featuresColor,
                            currentY
                        );
                        resultsContainer.Controls.Add(featuresBox);
                        currentY += featuresBox.Height + 10;
                    }

                    // TASKS
                    if (!string.IsNullOrWhiteSpace(result.RecommendedTasks))
                    {
                        var tasksBox = CreateResultBox(
                            "[+] RECOMMENDED TASKS",
                            FormatListItems(result.RecommendedTasks),
                            tasksColor,
                            currentY
                        );
                        resultsContainer.Controls.Add(tasksBox);
                        currentY += tasksBox.Height + 10;
                    }

                    // ROLES
                    if (!string.IsNullOrWhiteSpace(result.RequiredEmployeeTypes))
                    {
                        var rolesBox = CreateResultBox(
                            "[#] REQUIRED EMPLOYEE TYPES",
                            FormatListItems(result.RequiredEmployeeTypes),
                            rolesColor,
                            currentY
                        );
                        resultsContainer.Controls.Add(rolesBox);
                        currentY += rolesBox.Height + 10;
                    }

                    // TEAM
                    if (!string.IsNullOrWhiteSpace(result.TeamComposition))
                    {
                        var teamBox = CreateResultBox(
                            "[@] TEAM COMPOSITION",
                            result.TeamComposition,
                            teamColor,
                            currentY
                        );
                        resultsContainer.Controls.Add(teamBox);
                        currentY += teamBox.Height + 10;
                    }

                    // INSIGHTS
                    if (!string.IsNullOrWhiteSpace(result.Explanation))
                    {
                        var insightsBox = CreateResultBox(
                            "[!] AI INSIGHTS",
                            result.Explanation,
                            insightsColor,
                            currentY
                        );
                        resultsContainer.Controls.Add(insightsBox);
                    }

                    // Scroll to top
                    resultsContainer.AutoScrollPosition = new Point(0, 0);
                }
                catch (Exception ex)
                {
                    // Clear loading
                    resultsContainer.Controls.Clear();

                    // Show error with red theme
                    var errorBox = CreateResultBox(
                        "[X] ERROR",
                        $"Failed to get AI suggestions:\n{ex.Message}\n\nPlease check your Gemini API key in appsettings.json",
                        Color.FromArgb(211, 47, 47), // Red
                        10
                    );
                    resultsContainer.Controls.Add(errorBox);
                }
                finally
                {
                    // Reset button state
                    btnGetSuggestions.Enabled = true;
                    btnGetSuggestions.Text = "Get Suggestions";
                    progressBar.Visible = false;
                }
            };

            // ??????????????????????????????????????????
            // ADD ALL COMPONENTS TO AI PANEL
            // ??????????????????????????????????????????
            aiPanel.Controls.Add(headerPanel);
            aiPanel.Controls.Add(progressBar);
            aiPanel.Controls.Add(resultsContainer);

            // Add AI panel to main panel
            mainPanel.Controls.Add(aiPanel);

            int currentTop = 650;  // Adjusted for modern AI panel (was 440)

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
                currentTop += 95;
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
