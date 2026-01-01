using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Performance.Infrastructure.Entities
{
    public enum ProjectStatus { Active, Completed, OnHold, Cancelled }
    
    [Index(nameof(CreatedAt))]
    [Index(nameof(Status))]
    [Index(nameof(CreatedById))]
    public class ProjectEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ManagerNotes { get; set; }
        
        // New fields for better tracking
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;
        public string? CreatedById { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // Navigation properties
        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
        public UserEntity? CreatedBy { get; set; }
    }
}