using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Performance.Application.Services;
using Performance.Infrastructure.Entities;
using Performance.UI;
using TaskStatus = Performance.Infrastructure.Entities.TaskStatus;

namespace Performance
{
    public partial class TaskDetailForm : BaseForm
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly IUserService _userService;
        private readonly UserEntity? _currentUser;
        private TaskEntity _task;
        private System.Windows.Forms.Timer? _refreshTimer;

        // UI Controls
        private Label _lblTitle = null!;
        private Label _lblProject = null!;
        private Label _lblDescription = null!;
        private Label _lblDeadline = null!;
        private Label _lblAssignedTo = null!;
        private Label _lblEstimatedHours = null!;
        private ProgressBar _progressBar = null!;
        private Label _lblProgress = null!;
        private Panel _statusPanel = null!;
        private TextBox _txtManagerNotes = null!;
        private Button _btnTodo = null!;
        private Button _btnInProgress = null!;
        private Button _btnDone = null!;
        private Button _btnEdit = null!;
        private Button _btnClose = null!;

        private readonly IServiceProvider _serviceProvider;

        public TaskDetailForm(ITaskService taskService, IProjectService projectService, IUserService userService, IServiceProvider serviceProvider, TaskEntity task, UserEntity? currentUser = null)
        {
            _taskService = taskService;
            _projectService = projectService;
            _userService = userService;
            _serviceProvider = serviceProvider;
            _task = task;
            _currentUser = currentUser;
            InitializeComponent();
            LoadTaskDetails();
            StartRefreshTimer();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(700, 650);
            this.Name = "TaskDetailForm";
            this.Text = "Task Details";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UiColors.Background;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int yPos = 20;

            // Header Panel with Title
            var headerPanel = new Panel() 
            { 
                Left = 20, 
                Top = yPos, 
                Width = 660, 
                Height = 60, 
                BackColor = UiColors.DarkGreen 
            };
            _lblTitle = new Label() 
            { 
                Left = 15, 
                Top = 15, 
                Width = 630, 
                Height = 30,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false
            };
            headerPanel.Controls.Add(_lblTitle);
            this.Controls.Add(headerPanel);
            yPos += 70;

            // Info Grid
            var infoPanel = new TableLayoutPanel() 
            { 
                Left = 20, 
                Top = yPos, 
                Width = 660, 
                Height = 180,
                BackColor = Color.White,
                Padding = new Padding(15),
                ColumnCount = 2,
                RowCount = 5
            };
            infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddInfoRow(infoPanel, 0, "Project:", ref _lblProject);
            AddInfoRow(infoPanel, 1, "Assigned To:", ref _lblAssignedTo);
            AddInfoRow(infoPanel, 2, "Deadline:", ref _lblDeadline);
            AddInfoRow(infoPanel, 3, "Estimated Hours:", ref _lblEstimatedHours);
            AddInfoRow(infoPanel, 4, "Progress:", ref _lblProgress);

            this.Controls.Add(infoPanel);
            yPos += 190;

            // Progress Bar
            _progressBar = new ProgressBar() { Left = 20, Top = yPos, Width = 660, Height = 25 };
            this.Controls.Add(_progressBar);
            yPos += 35;

            // Description
            var lblDescTitle = new Label() 
            { 
                Text = "Description", 
                Left = 20, 
                Top = yPos, 
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = UiColors.PrimaryText,
                AutoSize = true
            };
            this.Controls.Add(lblDescTitle);
            yPos += 25;

            _lblDescription = new Label() 
            { 
                Left = 20, 
                Top = yPos, 
                Width = 660, 
                Height = 80,
                ForeColor = UiColors.SecondaryText,
                BackColor = Color.White,
                Padding = new Padding(10),
                AutoSize = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(_lblDescription);
            yPos += 90;

            // Manager Notes (if manager)
            if (_currentUser?.Role == UserRole.Manager)
            {
                var lblNotesTitle = new Label() 
                { 
                    Text = "Manager Notes", 
                    Left = 20, 
                    Top = yPos, 
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = UiColors.PrimaryText,
                    AutoSize = true
                };
                this.Controls.Add(lblNotesTitle);
                yPos += 25;

                _txtManagerNotes = new TextBox() 
                { 
                    Left = 20, 
                    Top = yPos, 
                    Width = 660, 
                    Height = 60,
                    Multiline = true
                };
                UiHelpers.StyleTextBox(_txtManagerNotes);
                this.Controls.Add(_txtManagerNotes);
                yPos += 70;
            }

            // Status Change Panel
            _statusPanel = new Panel() 
            { 
                Left = 20, 
                Top = yPos, 
                Width = 660, 
                Height = 50, 
                BackColor = UiColors.MediumGreen 
            };
            
            var lblStatus = new Label() 
            { 
                Text = "Change Status:", 
                Left = 10, 
                Top = 15, 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            _statusPanel.Controls.Add(lblStatus);

            _btnTodo = CreateStatusButton("Todo", 150, UiColors.StatusTodo);
            _btnInProgress = CreateStatusButton("In Progress", 250, UiColors.StatusInProgress);
            _btnDone = CreateStatusButton("Done", 380, UiColors.StatusDone);

            _btnTodo.Click += async (s, e) => await ChangeStatus(TaskStatus.Todo);
            _btnInProgress.Click += async (s, e) => await ChangeStatus(TaskStatus.InProgress);
            _btnDone.Click += async (s, e) => await ChangeStatus(TaskStatus.Done);

            _statusPanel.Controls.Add(_btnTodo);
            _statusPanel.Controls.Add(_btnInProgress);
            _statusPanel.Controls.Add(_btnDone);

            this.Controls.Add(_statusPanel);
            yPos += 60;

            // Action Buttons
            _btnEdit = new Button() { Text = "Edit Task", Left = 20, Top = yPos, Width = 120, Height = 35 };
            _btnClose = new Button() { Text = "Close", Left = 560, Top = yPos, Width = 120, Height = 35 };

            _btnEdit.Click += BtnEdit_Click;
            _btnClose.Click += (s, e) => this.Close();

            UiHelpers.ApplyButtonStyle(_btnEdit);
            UiHelpers.ApplyButtonStyle(_btnClose);

            // Hide edit button for employees
            if (_currentUser?.Role == UserRole.Employee)
            {
                _btnEdit.Visible = false;
            }

            this.Controls.Add(_btnEdit);
            this.Controls.Add(_btnClose);

            this.ResumeLayout(false);
        }

        private void AddInfoRow(TableLayoutPanel panel, int row, string labelText, ref Label valueLabel)
        {
            var lbl = new Label() 
            { 
                Text = labelText, 
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = UiColors.SecondaryText,
                Font = new Font("Segoe UI", 9F)
            };
            
            valueLabel = new Label() 
            { 
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            panel.Controls.Add(lbl, 0, row);
            panel.Controls.Add(valueLabel, 1, row);
        }

        private Button CreateStatusButton(string text, int left, Color backColor)
        {
            var btn = new Button() 
            { 
                Text = text, 
                Left = left, 
                Top = 10, 
                Width = 100, 
                Height = 30,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async void LoadTaskDetails()
        {
            try
            {
                // Reload task to get latest data
                _task = (await _taskService.GetAsync(_task.Id))!;

                _lblTitle.Text = _task.Title;
                _lblDescription.Text = _task.Description ?? "No description";
                
                // Load project name
                var project = await _projectService.GetAsync(_task.ProjectId);
                _lblProject.Text = project?.Name ?? "Unknown Project";

                // Load assigned user name instead of ID
                if (!string.IsNullOrEmpty(_task.AssignedToUserId))
                {
                    var assignedUser = await _userService.GetAsync(_task.AssignedToUserId);
                    _lblAssignedTo.Text = assignedUser?.UserName ?? _task.AssignedToUserId;
                }
                else
                {
                    _lblAssignedTo.Text = "Unassigned";
                }

                _lblEstimatedHours.Text = _task.EstimatedDuration.HasValue ? $"{_task.EstimatedDuration.Value.TotalHours:F1} hours" : "Not set";

                // Deadline with countdown
                UpdateDeadlineLabel();

                // Progress
                UpdateProgress();

                // Manager notes
                if (_txtManagerNotes != null)
                {
                    _txtManagerNotes.Text = _task.ManagerNotes ?? string.Empty;
                }

                // Highlight current status button
                HighlightCurrentStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load task details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDeadlineLabel()
        {
            if (_task.Deadline.HasValue)
            {
                var timeLeft = _task.Deadline.Value - DateTime.Now;
                if (timeLeft.TotalDays > 0)
                {
                    _lblDeadline.Text = $"{_task.Deadline.Value:yyyy-MM-dd HH:mm} ({timeLeft.Days}d {timeLeft.Hours}h remaining)";
                    _lblDeadline.ForeColor = timeLeft.TotalDays <= 2 ? UiColors.Error : UiColors.PrimaryText;
                }
                else
                {
                    _lblDeadline.Text = $"{_task.Deadline.Value:yyyy-MM-dd HH:mm} (OVERDUE)";
                    _lblDeadline.ForeColor = UiColors.Error;
                }
            }
            else
            {
                _lblDeadline.Text = "No deadline";
                _lblDeadline.ForeColor = UiColors.SecondaryText;
            }
        }

        private void UpdateProgress()
        {
            int progress = _task.Status switch
            {
                TaskStatus.Todo => 0,
                TaskStatus.InProgress => 50,
                TaskStatus.Done => 100,
                _ => 0
            };

            _progressBar.Value = progress;
            _lblProgress.Text = $"{progress}% ({_task.Status})";

            // Color based on status
            _progressBar.ForeColor = _task.Status switch
            {
                TaskStatus.Done => UiColors.StatusDone,
                TaskStatus.InProgress => UiColors.StatusInProgress,
                _ => UiColors.StatusTodo
            };
        }

        private void HighlightCurrentStatus()
        {
            _btnTodo.BackColor = _task.Status == TaskStatus.Todo ? UiColors.StatusTodo : UiColors.Darken(UiColors.MediumGreen, 0.3f);
            _btnInProgress.BackColor = _task.Status == TaskStatus.InProgress ? UiColors.StatusInProgress : UiColors.Darken(UiColors.MediumGreen, 0.3f);
            _btnDone.BackColor = _task.Status == TaskStatus.Done ? UiColors.StatusDone : UiColors.Darken(UiColors.MediumGreen, 0.3f);
        }

        private async System.Threading.Tasks.Task ChangeStatus(TaskStatus newStatus)
        {
            try
            {
                _task.Status = newStatus;

                // Save manager notes if changed
                if (_txtManagerNotes != null && _currentUser?.Role == UserRole.Manager)
                {
                    _task.ManagerNotes = _txtManagerNotes.Text;
                }

                await _taskService.UpdateAsync(_task);
                
                UpdateProgress();
                HighlightCurrentStatus();
                
                MessageBox.Show($"Task status changed to {newStatus}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to change status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var suggestionService = scope.ServiceProvider.GetRequiredService<ITaskSuggestionService>();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                
                var form = new TaskEditForm(suggestionService, taskService, userService, _currentUser);
                form.LoadTask(_task);
                
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadTaskDetails();
                    MessageBox.Show("Task updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open edit form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartRefreshTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 60000; // 1 minute
            _refreshTimer.Tick += (s, e) => UpdateDeadlineLabel();
            _refreshTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
