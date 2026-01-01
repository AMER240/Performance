using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.UI;
using TaskStatus = Performance.Domain.Enums.TaskStatus;

namespace Performance
{
    public class DashboardStatsPanel : Panel
    {
        private readonly IServiceProvider _serviceProvider;
        private UserEntity? _currentUser;

        private Label _lblTotalProjects = null!;
        private Label _lblTotalTasks = null!;
        private Label _lblCompletedTasks = null!;
        private Label _lblUpcomingDeadlines = null!;
        private Label _lblMyTasks = null!;

        public DashboardStatsPanel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializePanel();
        }

        public void SetCurrentUser(UserEntity? user)
        {
            _currentUser = user;
        }

        private void InitializePanel()
        {
            this.Dock = DockStyle.Top;
            this.Height = 140;
            this.BackColor = UiColors.MediumGreen;
            this.Padding = new Padding(20, 20, 20, 20);

            var layout = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                BackColor = UiColors.MediumGreen
            };

            // Total Projects
            var projectsCard = CreateStatCard("Total Projects", "0", UiColors.DarkGreen, "\U0001F4C1");
            _lblTotalProjects = (Label)projectsCard.Controls[2];

            // Total Tasks
            var tasksCard = CreateStatCard("Total Tasks", "0", UiColors.DarkestGreen, "\U0001F4CB");
            _lblTotalTasks = (Label)tasksCard.Controls[2];

            // Completed Tasks
            var completedCard = CreateStatCard("Completed", "0%", UiColors.Success, "\u2713");
            _lblCompletedTasks = (Label)completedCard.Controls[2];

            // Upcoming Deadlines
            var deadlinesCard = CreateStatCard("Due Soon", "0", UiColors.Warning, "\u23F0");
            _lblUpcomingDeadlines = (Label)deadlinesCard.Controls[2];

            // My Tasks
            var myTasksCard = CreateStatCard("My Tasks", "0", UiColors.Info, "\U0001F464");
            _lblMyTasks = (Label)myTasksCard.Controls[2];

            layout.Controls.Add(projectsCard);
            layout.Controls.Add(tasksCard);
            layout.Controls.Add(completedCard);
            layout.Controls.Add(deadlinesCard);
            layout.Controls.Add(myTasksCard);

            this.Controls.Add(layout);
        }

        private Panel CreateStatCard(string title, string value, Color accentColor, string icon)
        {
            var card = new Panel()
            {
                Width = 220,
                Height = 100,
                Margin = new Padding(5),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var accent = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = accentColor
            };

            var lblTitle = new Label()
            {
                Text = title.ToUpperInvariant(),
                ForeColor = UiColors.SecondaryText,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Left = 15,
                Top = 15,
                AutoSize = true
            };

            var lblValue = new Label()
            {
                Text = value,
                ForeColor = UiColors.PrimaryText,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                Left = 15,
                Top = 45,
                AutoSize = true
            };

            // Icon with simple text/symbol that works across all systems
            var lblIcon = new Label()
            {
                Text = icon,
                ForeColor = accentColor,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Left = 175,
                Top = 12,
                Width = 40,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(accent);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblIcon);

            return card;
        }

        public async System.Threading.Tasks.Task RefreshStatistics()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

                var projects = await projectService.ListAsync();
                var allTasks = new System.Collections.Generic.List<TaskEntity>();

                foreach (var project in projects)
                {
                    var tasks = await taskService.ListByProjectAsync(project.Id);
                    allTasks.AddRange(tasks);
                }

                _lblTotalProjects.Text = projects.Count.ToString();
                _lblTotalTasks.Text = allTasks.Count.ToString();

                if (allTasks.Count > 0)
                {
                    var completed = allTasks.Count(t => t.Status == TaskStatus.Done);
                    var percentage = (int)((completed / (double)allTasks.Count) * 100);
                    _lblCompletedTasks.Text = $"{percentage}%";
                }
                else
                {
                    _lblCompletedTasks.Text = "0%";
                }

                var upcomingDeadlines = allTasks.Count(t => 
                    t.Deadline.HasValue && 
                    t.Deadline.Value >= DateTime.Now && 
                    t.Deadline.Value <= DateTime.Now.AddDays(7) &&
                    t.Status != TaskStatus.Done);
                _lblUpcomingDeadlines.Text = upcomingDeadlines.ToString();

                if (_currentUser != null)
                {
                    var myTasks = allTasks.Count(t => t.AssignedToUserId == _currentUser.Id.ToString());
                    _lblMyTasks.Text = myTasks.ToString();
                }
                else
                {
                    _lblMyTasks.Text = "-";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load statistics: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
