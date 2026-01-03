using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.UI;
using TaskStatus = Performance.Domain.Enums.TaskStatus;

namespace Performance
{
    public partial class TaskListForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly ITaskService _taskService;
        private readonly IServiceProvider _provider;
        private readonly IUserService _userService;
        private DataGridView _grid = new DataGridView();
        private int _projectId;
        private UserEntity? _currentUser;
        
        // Filter controls
        private ComboBox _cmbStatusFilter = new ComboBox();
        private ComboBox _cmbPriorityFilter = new ComboBox();
        private TextBox _txtSearch = new TextBox();
        private Label _lblTaskCount = new Label();
        
        // Action buttons
        private Button _btnAdd = null!;
        private Button _btnEdit = null!;
        private Button _btnDelete = null!;
        private Button _btnDetails = null!;
        private Button _btnRefresh = null!;

        public TaskListForm(ITaskService taskService, IServiceProvider provider, IUserService userService)
        {
            _taskService = taskService;
            _provider = provider;
            _userService = userService;
            InitializeComponent();
        }

        public void SetCurrentUser(UserEntity? user)
        {
            _currentUser = user;
            
            // Hide task management buttons for employees and move remaining buttons left
            if (_currentUser?.Role == UserRole.Employee)
            {
                _btnAdd.Visible = false;
                _btnEdit.Visible = false;
                _btnDelete.Visible = false;
                
                // Move View Details and Refresh to the left
                _btnDetails.Left = 15;
                _btnRefresh.Left = 155;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Name = "TaskListForm";
            this.Text = "Task Management";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UiColors.Background;

            // Top panel with filters
            var topPanel = new Panel() 
            { 
                Dock = DockStyle.Top, 
                Height = 80, 
                BackColor = UiColors.MediumGreen,
                Padding = new Padding(15)
            };

            // Search box
            var lblSearch = new Label() 
            { 
                Text = "Search:", 
                Left = 15, 
                Top = 15, 
                ForeColor = Color.White,
                AutoSize = true
            };
            _txtSearch = new TextBox() 
            { 
                Left = 80, 
                Top = 12, 
                Width = 250
            };
            UiHelpers.StyleTextBox(_txtSearch);
            try { _txtSearch.PlaceholderText = "Search tasks..."; } catch { }
            _txtSearch.TextChanged += async (s, e) => await ApplyFilters();

            // Status filter
            var lblStatus = new Label() 
            { 
                Text = "Status:", 
                Left = 350, 
                Top = 15, 
                ForeColor = Color.White,
                AutoSize = true
            };
            _cmbStatusFilter = new ComboBox() 
            { 
                Left = 410, 
                Top = 12, 
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            UiHelpers.StyleComboBox(_cmbStatusFilter);
            _cmbStatusFilter.Items.AddRange(new object[] { "All", "Todo", "In Progress", "Done" });
            _cmbStatusFilter.SelectedIndex = 0;
            _cmbStatusFilter.SelectedIndexChanged += async (s, e) => await ApplyFilters();

            // Priority filter
            var lblPriority = new Label() 
            { 
                Text = "Priority:", 
                Left = 550, 
                Top = 15, 
                ForeColor = Color.White,
                AutoSize = true
            };
            _cmbPriorityFilter = new ComboBox() 
            { 
                Left = 620, 
                Top = 12, 
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            UiHelpers.StyleComboBox(_cmbPriorityFilter);
            _cmbPriorityFilter.Items.AddRange(new object[] { "All", "Low", "Medium", "High" });
            _cmbPriorityFilter.SelectedIndex = 0;
            _cmbPriorityFilter.SelectedIndexChanged += async (s, e) => await ApplyFilters();

            // Task count label
            _lblTaskCount = new Label() 
            { 
                Text = "0 tasks", 
                Left = 760, 
                Top = 15, 
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                AutoSize = true
            };

            topPanel.Controls.Add(lblSearch);
            topPanel.Controls.Add(_txtSearch);
            topPanel.Controls.Add(lblStatus);
            topPanel.Controls.Add(_cmbStatusFilter);
            topPanel.Controls.Add(lblPriority);
            topPanel.Controls.Add(_cmbPriorityFilter);
            topPanel.Controls.Add(_lblTaskCount);

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
            _grid.RowTemplate.Height = 35;

            // Apply UiHelpers styling
            UiHelpers.StyleGrid(_grid);

            // Columns - ID and Assigned To are hidden
            _grid.Columns.Add(new DataGridViewTextBoxColumn() 
            { 
                DataPropertyName = "Id", 
                HeaderText = "ID",
                Visible = false  // HIDDEN
            });
            
            _grid.Columns.Add(new DataGridViewTextBoxColumn() 
            { 
                DataPropertyName = "Title", 
                HeaderText = "Title", 
                Width = 450,
                DefaultCellStyle = new DataGridViewCellStyle() { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });
            
            _grid.Columns.Add(new DataGridViewTextBoxColumn() 
            { 
                DataPropertyName = "Status", 
                HeaderText = "Status", 
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            
            _grid.Columns.Add(new DataGridViewTextBoxColumn() 
            { 
                DataPropertyName = "Priority", 
                HeaderText = "Priority", 
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            
            _grid.Columns.Add(new DataGridViewTextBoxColumn() 
            { 
                DataPropertyName = "AssignedToUserId", 
                HeaderText = "Assigned To",
                Visible = false  // HIDDEN
            });
            
            _grid.Columns.Add(new DataGridViewTextBoxColumn() 
            { 
                DataPropertyName = "Deadline", 
                HeaderText = "Deadline", 
                Width = 200,
                DefaultCellStyle = new DataGridViewCellStyle() { Format = "yyyy-MM-dd HH:mm" }
            });

            // Row coloring based on status/priority
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.DoubleClick += async (s, e) => await ViewDetails_Click();

            // Bottom panel with action buttons
            var bottomPanel = new Panel() 
            { 
                Dock = DockStyle.Bottom, 
                Height = 70, 
                BackColor = UiColors.MediumGreen,
                Padding = new Padding(15)
            };

            _btnAdd = new Button() 
            { 
                Text = "Add Task", 
                Left = 15, 
                Top = 20, 
                Width = 120, 
                Height = 35 
            };
            _btnEdit = new Button() 
            { 
                Text = "Edit", 
                Left = 145, 
                Top = 20, 
                Width = 100, 
                Height = 35 
            };
            _btnDelete = new Button() 
            { 
                Text = "Delete", 
                Left = 255, 
                Top = 20, 
                Width = 100, 
                Height = 35 
            };
            _btnDetails = new Button() 
            { 
                Text = "View Details", 
                Left = 365, 
                Top = 20, 
                Width = 130, 
                Height = 35 
            };
            _btnRefresh = new Button() 
            { 
                Text = "Refresh", 
                Left = 505, 
                Top = 20, 
                Width = 100, 
                Height = 35 
            };

            _btnAdd.Click += async (s, e) => await Add_Click();
            _btnEdit.Click += async (s, e) => await Edit_Click();
            _btnDelete.Click += async (s, e) => await Delete_Click();
            _btnDetails.Click += async (s, e) => await ViewDetails_Click();
            _btnRefresh.Click += async (s, e) => await RefreshList();

            UiHelpers.ApplyButtonStyle(_btnAdd);
            UiHelpers.ApplyButtonStyle(_btnEdit);
            UiHelpers.ApplyButtonStyle(_btnDelete);
            UiHelpers.ApplyButtonStyle(_btnDetails);
            UiHelpers.ApplyButtonStyle(_btnRefresh);

            bottomPanel.Controls.Add(_btnAdd);
            bottomPanel.Controls.Add(_btnEdit);
            bottomPanel.Controls.Add(_btnDelete);
            bottomPanel.Controls.Add(_btnDetails);
            bottomPanel.Controls.Add(_btnRefresh);

            this.Controls.Add(_grid);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);

            this.Load += async (s, e) => await TaskListForm_Load();
            this.ResumeLayout(false);
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Rows[e.RowIndex].DataBoundItem is not TaskEntity task) return;

            // Color based on status
            if (_grid.Columns[e.ColumnIndex].HeaderText == "Status")
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
            if (_grid.Columns[e.ColumnIndex].HeaderText == "Priority")
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

            // Highlight overdue deadlines
            if (_grid.Columns[e.ColumnIndex].HeaderText == "Deadline" && task.Deadline.HasValue)
            {
                if (task.Deadline.Value < DateTime.Now && task.Status != TaskStatus.Done)
                {
                    e.CellStyle.BackColor = UiColors.Darken(UiColors.Error, 0.7f);
                    e.CellStyle.ForeColor = UiColors.Error;
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            }
        }

        private async Task TaskListForm_Load()
        {
            await RefreshList();
        }

        public void SetProject(int projectId)
        {
            _projectId = projectId;
        }

        private async Task RefreshList()
        {
            if (_projectId == 0) return;

            try
            {
                // Include relations to avoid N+1 problem
                var tasks = await _taskService.ListByProjectAsync(_projectId, includeRelations: true);

                // Ensure grid is not disposed
                if (_grid.IsDisposed) return;

                _grid.DataSource = tasks;
                UpdateTaskCount(tasks.Count);
            }
            catch (Exception ex)
            {
                if (!this.IsDisposed)
                {
                    MessageBox.Show($"Failed to load tasks: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task ApplyFilters()
        {
            if (_projectId == 0) return;

            try  //  EKLE
            {
                var tasks = await _taskService.ListByProjectAsync(_projectId, includeRelations: true);

                // ... mevcut kod ...

                if (!_grid.IsDisposed)  //  EKLE
                {
                    _grid.DataSource = tasks;
                    UpdateTaskCount(tasks.Count);
                }
            }
            catch (Exception ex)
            {
                if (!this.IsDisposed)
                {
                    MessageBox.Show($"Failed to apply filters: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateTaskCount(int count)
        {
            _lblTaskCount.Text = $"{count} task{(count != 1 ? "s" : "")}";
        }

        private async Task Add_Click()
        {
            using var scope = _provider.CreateScope();
            var suggestionService = scope.ServiceProvider.GetRequiredService<ITaskSuggestionService>();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var form = new TaskEditForm(suggestionService, taskService, userService, _currentUser);
            form.SetProject(_projectId);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await ApplyFilters();
            }
        }

        private async Task Edit_Click()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a task to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var task = (TaskEntity)_grid.SelectedRows[0].DataBoundItem;
            using var scope = _provider.CreateScope();
            var suggestionService = scope.ServiceProvider.GetRequiredService<ITaskSuggestionService>();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var form = new TaskEditForm(suggestionService, taskService, userService, _currentUser);
            form.LoadTask(task);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await ApplyFilters();
            }
        }

        private async Task Delete_Click()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a task to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var task = (TaskEntity)_grid.SelectedRows[0].DataBoundItem;
            var r = MessageBox.Show($"Delete task '{task.Title}'?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;
            
            await _taskService.DeleteAsync(task.Id);
            await ApplyFilters();
        }

        private async Task ViewDetails_Click()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a task to view details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var task = (TaskEntity)_grid.SelectedRows[0].DataBoundItem;
            
            using var scope = _provider.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
            var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            
            var detailForm = new TaskDetailForm(taskService, projectService, userService, _provider, task, _currentUser);
            detailForm.ShowDialog(this);
            
            await ApplyFilters();
        }
    }
}
