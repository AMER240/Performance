using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Infrastructure.Entities;

namespace Performance.Application.Services
{
    public interface ITaskService
    {
        // Basic CRUD
        Task<List<TaskEntity>> ListAsync(bool includeRelations = false); // Get all tasks
        Task<List<TaskEntity>> ListByProjectAsync(int projectId, bool includeRelations = false);
        Task<List<TaskEntity>> ListByUserAsync(string userId, bool includeRelations = false);
        Task<TaskEntity?> GetAsync(int id, bool includeRelations = true);
        Task<TaskEntity> CreateAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id); // New: Soft delete
        
        // Pagination
        Task<(List<TaskEntity> Tasks, int TotalCount)> ListPagedAsync(int pageNumber, int pageSize, bool includeRelations = false);
        
        // Filtered queries
        Task<List<TaskEntity>> ListByStatusAsync(Infrastructure.Entities.TaskStatus status, bool includeRelations = false);
        Task<List<TaskEntity>> ListByPriorityAsync(Infrastructure.Entities.TaskPriority priority, bool includeRelations = false);
        Task<List<TaskEntity>> ListUpcomingDeadlinesAsync(int days, bool includeRelations = false);
    }
}
