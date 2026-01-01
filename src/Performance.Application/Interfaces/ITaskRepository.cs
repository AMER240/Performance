using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Domain.Entities;
using TaskStatus = Performance.Domain.Enums.TaskStatus;

namespace Performance.Application.Interfaces
{
    /// <summary>
    /// Task-specific repository interface with domain-specific queries
    /// </summary>
    public interface ITaskRepository : IRepository<TaskEntity>
    {
        /// <summary>
        /// Get completed tasks for historical analysis
        /// </summary>
        Task<List<TaskEntity>> GetCompletedTasksAsync(int? projectId = null, int maxCount = 100);
        
        /// <summary>
        /// Get tasks by project ID
        /// </summary>
        Task<List<TaskEntity>> GetTasksByProjectIdAsync(int projectId);
        
        /// <summary>
        /// Get tasks assigned to a specific user
        /// </summary>
        Task<List<TaskEntity>> GetTasksByUserIdAsync(string userId);
        
        /// <summary>
        /// Get active users in a project (users with assigned tasks)
        /// </summary>
        Task<List<string>> GetActiveUsersInProjectAsync(int projectId, int maxCount = 3);
        
        /// <summary>
        /// Get tasks by status
        /// </summary>
        Task<List<TaskEntity>> GetTasksByStatusAsync(TaskStatus status);
    }
}
