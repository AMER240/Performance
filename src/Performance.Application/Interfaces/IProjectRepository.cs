using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Domain.Entities;
using Performance.Domain.Enums;

namespace Performance.Application.Interfaces
{
    /// <summary>
    /// Project-specific repository interface
    /// </summary>
    public interface IProjectRepository : IRepository<ProjectEntity>
    {
        /// <summary>
        /// Get projects by status
        /// </summary>
        Task<List<ProjectEntity>> GetProjectsByStatusAsync(ProjectStatus status);
        
        /// <summary>
        /// Search projects by name
        /// </summary>
        Task<List<ProjectEntity>> SearchProjectsByNameAsync(string searchTerm);
        
        /// <summary>
        /// Get project with its tasks
        /// </summary>
        Task<ProjectEntity?> GetProjectWithTasksAsync(int projectId);
    }
}
