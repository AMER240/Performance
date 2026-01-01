using System;
using System.Collections.Generic;
using Performance.Domain.Enums;

namespace Performance.Domain.Entities
{
    public class ProjectEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ManagerNotes { get; set; }
        
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;
        public string? CreatedById { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
        public UserEntity? CreatedBy { get; set; }
    }
}
