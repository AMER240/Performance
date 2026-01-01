using System;
using Microsoft.EntityFrameworkCore;

namespace Performance.Infrastructure.Entities
{
    public enum TaskStatus { Todo, InProgress, Done }
    public enum TaskPriority { Low, Medium, High }
    
    [Index(nameof(ProjectId))]
    [Index(nameof(Status))]
    [Index(nameof(Priority))]
    [Index(nameof(Deadline))]
    [Index(nameof(AssignedToUserId))]
    [Index(nameof(IsDeleted))]
    [Index(nameof(Status), nameof(Priority))]
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
        
        // Soft delete support
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        
        // Timestamps for better tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // Navigation properties
        public ProjectEntity Project { get; set; } = null!;
        public UserEntity? AssignedToUser { get; set; }
    }
}