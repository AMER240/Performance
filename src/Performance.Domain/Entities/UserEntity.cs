using System.Collections.Generic;
using Performance.Domain.Enums;

namespace Performance.Domain.Entities
{
    public class UserEntity
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? Sector { get; set; }
        
        public ICollection<TaskEntity> AssignedTasks { get; set; } = new List<TaskEntity>();
        public ICollection<ProjectEntity> ManagedProjects { get; set; } = new List<ProjectEntity>();
    }
}
