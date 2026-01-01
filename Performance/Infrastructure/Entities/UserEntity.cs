using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Performance.Infrastructure.Entities
{
    public enum UserRole { Employee, Manager }
    
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(Sector))]
    [Index(nameof(Role))]
    public class UserEntity
    {
        public string Id { get; set; } = string.Empty; // simple string id
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        
        // Profile fields
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? ProfilePhoto { get; set; } // Base64 encoded image or file path
        public string? Sector { get; set; } // User's sector/field (e.g., IT, Marketing, Finance)
        
        // Navigation properties
        public ICollection<TaskEntity> AssignedTasks { get; set; } = new List<TaskEntity>();
        public ICollection<ProjectEntity> ManagedProjects { get; set; } = new List<ProjectEntity>();
    }
}