using System;
using System.Collections.Generic;

namespace Performance.Infrastructure.Entities
{
    public enum UserRole { Employee, Manager }
    public class UserEntity
    {
        public string Id { get; set; } = null!; // use string to map to GUID or identity in future
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!; // simple hash storage
        public UserRole Role { get; set; }
        
        // Profile fields
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? ProfilePhoto { get; set; } // Base64 encoded image or file path
        public string? Sector { get; set; } // User's sector/field (e.g., IT, Marketing, Finance)
        
        public ICollection<TaskEntity> AssignedTasks { get; set; } = new List<TaskEntity>();
    }
}