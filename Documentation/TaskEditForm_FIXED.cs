using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Performance.Application.Services;
using Performance.Infrastructure.Entities;
using Performance.UI;

namespace Performance
{
    public partial class TaskEditForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly ITaskSuggestionService _suggestionService;
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly UserEntity? _currentUser;
        private TaskEntity? _task;
        private int _projectId;
        
        // ? Cache için field
        private System.Collections.Generic.List<UserEntity>? _cachedUsers;

        private TextBox txtDesc = new TextBox();
        private TextBox txtTitle = new TextBox();
        private ComboBox cboPriority = new ComboBox();
        private ComboBox cboStatus = new ComboBox();
        private ComboBox cboAssignedUser = new ComboBox();
        private DateTimePicker dtpDeadline = new DateTimePicker();
        private TextBox txtEstimatedHours = new TextBox();
        private TextBox txtManagerNotes = new TextBox();
        private Button btnSuggest;
        private Label lblResult;

        public TaskEditForm(ITaskSuggestionService suggestionService, ITaskService taskService, IUserService userService, UserEntity? currentUser = null)
        {
            _suggestionService = suggestionService;
            _taskService = taskService;
            _userService = userService;
            _currentUser = currentUser;
            InitializeComponent();
        }

        private async void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(650, 620);
            this.Name = "TaskEditForm";
            this.Text = "Task Editor";
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
                Text = "TASK DETAILS",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Left = 20,
                Top = 14,
                AutoSize = true
            };
            titlePanel.Controls.Add(lblFormTitle);
            this.Controls.Add(titlePanel);

            // Main panel with fields
            var mainPanel = new Panel()
            {
                Top = 50,
                Left = 0,
                Width = 650,
                Height = 520,
                Padding = new Padding(20),
                BackColor = UiColors.Background,
                AutoScroll = true
            };

            int yPos = 10;
            int labelWidth = 120;
            int controlLeft = 130;

            var lblTitle = new Label() 
            { 
                Left = 10, 
                Top = yPos, 
                Text = "Title:", 
                Width = labelWidth,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            txtTitle = new TextBox() { Left = controlLeft, Top = yPos, Width = 480 };
            UiHelpers.StyleTextBox(txtTitle);
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(txtTitle);
            yPos += 35;

            var lblDesc = new Label() 
            { 
                Left = 10, 
                Top = yPos, 
                Text = "Description:", 
                Width = labelWidth,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            txtDesc = new TextBox() { Left = controlLeft, Top = yPos, Width = 480, Height = 100, Multiline = true };
            UiHelpers.StyleTextBox(txtDesc);
            mainPanel.Controls.Add(lblDesc);
            mainPanel.Controls.Add(txtDesc);
            yPos += 110;

            var lblPriority = new Label() 
            { 
                Left = 10, 
                Top = yPos, 
                Text = "Priority:", 
                Width = labelWidth,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            cboPriority = new ComboBox() { Left = controlLeft, Top = yPos, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            UiHelpers.StyleComboBox(cboPriority);
            cboPriority.Items.AddRange(new[] { "Low", "Medium", "High" });
            cboPriority.SelectedIndex = 1;
            mainPanel.Controls.Add(lblPriority);
            mainPanel.Controls.Add(cboPriority);

            var lblStatus = new Label() 
            { 
                Left = 300, 
                Top = yPos, 
                Text = "Status:", 
                Width = 80,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            cboStatus = new ComboBox() { Left = 380, Top = yPos, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            UiHelpers.StyleComboBox(cboStatus);
            cboStatus.Items.AddRange(new[] { "Todo", "InProgress", "Done" });
            cboStatus.SelectedIndex = 0;
            mainPanel.Controls.Add(lblStatus);
            mainPanel.Controls.Add(cboStatus);
            yPos += 35;

            var lblDeadline = new Label() 
            { 
                Left = 10, 
                Top = yPos, 
                Text = "Deadline:", 
                Width = labelWidth,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            dtpDeadline = new DateTimePicker() 
            { 
                Left = controlLeft, 
                Top = yPos, 
                Width = 200, 
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F)
            };
            dtpDeadline.ShowCheckBox = true;
            dtpDeadline.Checked = false;
            mainPanel.Controls.Add(lblDeadline);
            mainPanel.Controls.Add(dtpDeadline);
            yPos += 35;

            var lblEstimated = new Label() 
            { 
                Left = 10, 
                Top = yPos, 
                Text = "Est. Hours:", 
                Width = labelWidth,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            txtEstimatedHours = new TextBox() { Left = controlLeft, Top = yPos, Width = 100 };
            UiHelpers.StyleTextBox(txtEstimatedHours);
            mainPanel.Controls.Add(lblEstimated);
            mainPanel.Controls.Add(txtEstimatedHours);
            yPos += 35;

            var lblAssigned = new Label() 
            { 
                Left = 10, 
                Top = yPos, 
                Text = "Assigned To:", 
                Width = labelWidth,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            cboAssignedUser = new ComboBox() { Left = controlLeft, Top = yPos, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            UiHelpers.StyleComboBox(cboAssignedUser);
            cboAssignedUser.Items.Add("(Unassigned)");
            await LoadUsers();
            mainPanel.Controls.Add(lblAssigned);
            mainPanel.Controls.Add(cboAssignedUser);
            yPos += 35;

            // AI Suggestion panel
            var aiPanel = new Panel()
            {
                Left = 10,
                Top = yPos,
                Width = 600,
                Height = 80,
                BackColor = UiColors.Lighten(UiColors.Info, 0.8f),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSuggest = new Button() 
            { 
                Left = 10, 
                Top = 10, 
                Width = 140, 
                Height = 30,
                Text = "AI Suggest" 
            };
            UiHelpers.ApplyButtonStyle(btnSuggest);
            btnSuggest.BackColor = UiColors.Info;

            lblResult = new Label() 
            { 
                Left = 10, 
                Top = 45, 
                Width = 570, 
                Height = 25, 
                ForeColor = UiColors.SecondaryText,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };

            aiPanel.Controls.Add(btnSuggest);
            aiPanel.Controls.Add(lblResult);
            mainPanel.Controls.Add(aiPanel);
            yPos += 90;

            // Manager notes (only visible for managers)
            bool isManager = _currentUser?.Role == UserRole.Manager;
            if (isManager)
            {
                var lblNotes = new Label() 
                { 
                    Left = 10, 
                    Top = yPos, 
                    Text = "Manager Notes:", 
                    Width = labelWidth,
                    ForeColor = UiColors.PrimaryText,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                txtManagerNotes = new TextBox() { Left = controlLeft, Top = yPos, Width = 480, Height = 60, Multiline = true };
                UiHelpers.StyleTextBox(txtManagerNotes);
                mainPanel.Controls.Add(lblNotes);
                mainPanel.Controls.Add(txtManagerNotes);
                yPos += 70;
            }

            this.Controls.Add(mainPanel);

            // Bottom panel with buttons
            var bottomPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = UiColors.MediumGreen
            };

            var btnSave = new Button() 
            { 
                Left = 420, 
                Top = 12, 
                Width = 100, 
                Height = 35,
                Text = "Save" 
            };
            var btnCancel = new Button() 
            { 
                Left = 530, 
                Top = 12, 
                Width = 100, 
                Height = 35,
                Text = "Cancel" 
            };

            btnSuggest.Click += async (s, e) => await Suggest_Click();
            btnSave.Click += async (s, e) => await Save_Click();
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            UiHelpers.ApplyButtonStyle(btnSave);
            UiHelpers.ApplyButtonStyle(btnCancel);

            bottomPanel.Controls.Add(btnSave);
            bottomPanel.Controls.Add(btnCancel);
            this.Controls.Add(bottomPanel);

            this.ResumeLayout(false);
        }

        private async Task LoadUsers()
        {
            try
            {
                // ? Load users only once and cache
                if (_cachedUsers == null)
                {
                    _cachedUsers = await _userService.GetAllUsersAsync();
                }

                foreach (var user in _cachedUsers)
                {
                    cboAssignedUser.Items.Add($"{user.UserName} ({user.Role}) - {user.Id}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetProject(int projectId)
        {
            _projectId = projectId;
        }

        public async void LoadTask(TaskEntity task)
        {
            _task = task;
            _projectId = task.ProjectId;

            // ? Reload task with relations to ensure navigation properties are loaded
            var fullTask = await _taskService.GetAsync(task.Id, includeRelations: true);
            if (fullTask != null)
            {
                _task = fullTask;
            }

            txtTitle.Text = _task.Title;
            txtDesc.Text = _task.Description ?? string.Empty;
            cboPriority.SelectedItem = _task.Priority.ToString();
            cboStatus.SelectedItem = _task.Status.ToString();

            if (_task.Deadline.HasValue)
            {
                dtpDeadline.Value = _task.Deadline.Value;
                dtpDeadline.Checked = true;
            }
            else
            {
                dtpDeadline.Checked = false;
            }

            if (_task.EstimatedDuration.HasValue)
            {
                txtEstimatedHours.Text = _task.EstimatedDuration.Value.TotalHours.ToString("F1");
            }

            // ? Set assigned user - now with navigation property support
            if (!string.IsNullOrEmpty(_task.AssignedToUserId))
            {
                // Try using navigation property first
                if (_task.AssignedToUser != null)
                {
                    var userDisplay = $"{_task.AssignedToUser.UserName} ({_task.AssignedToUser.Role}) - {_task.AssignedToUser.Id}";
                    for (int i = 0; i < cboAssignedUser.Items.Count; i++)
                    {
                        if (cboAssignedUser.Items[i].ToString() == userDisplay)
                        {
                            cboAssignedUser.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    // Fallback to ID matching
                    for (int i = 0; i < cboAssignedUser.Items.Count; i++)
                    {
                        var item = cboAssignedUser.Items[i].ToString();
                        if (item != null && item.Contains(_task.AssignedToUserId))
                        {
                            cboAssignedUser.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                cboAssignedUser.SelectedIndex = 0; // Unassigned
            }

            if (_currentUser?.Role == UserRole.Manager && txtManagerNotes != null)
            {
                txtManagerNotes.Text = _task.ManagerNotes ?? string.Empty;
            }
        }

        private async Task Suggest_Click()
        {
            btnSuggest.Enabled = false;
            try
            {
                var r = await _suggestionService.SuggestAsync(txtDesc.Text, _projectId == 0 ? null : _projectId);
                cboPriority.SelectedItem = r.SuggestedPriority.ToString();
                txtEstimatedHours.Text = r.EstimatedDuration.TotalHours.ToString("F1");
                
                // Try to select suggested user
                if (r.SuggestedUserIds.Length > 0)
                {
                    var suggestedUserId = r.SuggestedUserIds[0];
                    for (int i = 0; i < cboAssignedUser.Items.Count; i++)
                    {
                        var item = cboAssignedUser.Items[i].ToString();
                        if (item != null && item.Contains(suggestedUserId))
                        {
                            cboAssignedUser.SelectedIndex = i;
                            break;
                        }
                    }
                }

                lblResult.Text = $"AI Suggestion: {r.Explanation} | Estimated: {r.EstimatedDuration.TotalHours:F1} hours | Priority: {r.SuggestedPriority}";
            }
            catch (Exception ex)
            {
                lblResult.Text = $"Error: {ex.Message}";
            }
            btnSuggest.Enabled = true;
        }

        private async Task Save_Click()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Title required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse assigned user ID
            string? assignedUserId = null;
            if (cboAssignedUser.SelectedIndex > 0 && cboAssignedUser.SelectedItem != null)
            {
                var selectedItem = cboAssignedUser.SelectedItem.ToString();
                if (selectedItem != null && selectedItem.Contains(" - "))
                {
                    assignedUserId = selectedItem.Split(new[] { " - " }, StringSplitOptions.None).Last();
                }
            }

            // Parse estimated duration
            TimeSpan? estimatedDuration = null;
            if (double.TryParse(txtEstimatedHours.Text, out double hours) && hours > 0)
            {
                estimatedDuration = TimeSpan.FromHours(hours);
            }

            // Parse deadline
            DateTime? deadline = null;
            if (dtpDeadline.Checked)
            {
                deadline = dtpDeadline.Value.Date;
            }

            try
            {
                if (_task == null)
                {
                    var t = new TaskEntity
                    {
                        ProjectId = _projectId,
                        Title = txtTitle.Text.Trim(),
                        Description = txtDesc.Text.Trim(),
                        Priority = Enum.Parse<TaskPriority>(cboPriority.SelectedItem?.ToString() ?? "Medium"),
                        Status = Enum.Parse<Infrastructure.Entities.TaskStatus>(cboStatus.SelectedItem?.ToString() ?? "Todo"),
                        AssignedToUserId = assignedUserId,
                        Deadline = deadline,
                        EstimatedDuration = estimatedDuration,
                        ManagerNotes = _currentUser?.Role == UserRole.Manager ? txtManagerNotes?.Text?.Trim() : null
                    };
                    await _taskService.CreateAsync(t);
                }
                else
                {
                    _task.Title = txtTitle.Text.Trim();
                    _task.Description = txtDesc.Text.Trim();
                    _task.Priority = Enum.Parse<TaskPriority>(cboPriority.SelectedItem?.ToString() ?? "Medium");
                    _task.Status = Enum.Parse<Infrastructure.Entities.TaskStatus>(cboStatus.SelectedItem?.ToString() ?? "Todo");
                    _task.AssignedToUserId = assignedUserId;
                    _task.Deadline = deadline;
                    _task.EstimatedDuration = estimatedDuration;
                    if (_currentUser?.Role == UserRole.Manager && txtManagerNotes != null)
                    {
                        _task.ManagerNotes = txtManagerNotes.Text.Trim();
                    }
                    await _taskService.UpdateAsync(_task);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save task: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
