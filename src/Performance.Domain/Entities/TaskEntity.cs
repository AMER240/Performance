using System;
using TaskStatus = Performance.Domain.Enums.TaskStatus;
using TaskPriority = Performance.Domain.Enums.TaskPriority;

namespace Performance.Domain.Entities
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? ManagerNotes { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public ProjectEntity Project { get; set; } = null!;
        public UserEntity? AssignedToUser { get; set; }
    }
}
