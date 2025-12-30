using System;

namespace Performance.Infrastructure.Entities
{
    public enum TaskStatus { Todo, InProgress, Done }
    public enum TaskPriority { Low, Medium, High }
    public class TaskEntity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public ProjectEntity Project { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? ManagerNotes { get; set; }
    }
}