using System;
using System.Collections.Generic;

namespace Performance.Infrastructure.Entities
{
    public class ProjectEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ManagerNotes { get; set; }
        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    }
}