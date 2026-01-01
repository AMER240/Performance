using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Domain.Entities;
using TaskStatus = Performance.Domain.Enums.TaskStatus;
using TaskPriority = Performance.Domain.Enums.TaskPriority;

namespace Performance.Application.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskEntity>> ListAsync(bool includeRelations = false);
        Task<List<TaskEntity>> ListByProjectAsync(int projectId, bool includeRelations = false);
        Task<List<TaskEntity>> ListByUserAsync(string userId, bool includeRelations = false);
        Task<TaskEntity?> GetAsync(int id, bool includeRelations = true);
        Task<TaskEntity> CreateAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        
        Task<(List<TaskEntity> Tasks, int TotalCount)> ListPagedAsync(int pageNumber, int pageSize, bool includeRelations = false);
        
        Task<List<TaskEntity>> ListByStatusAsync(TaskStatus status, bool includeRelations = false);
        Task<List<TaskEntity>> ListByPriorityAsync(TaskPriority priority, bool includeRelations = false);
        Task<List<TaskEntity>> ListUpcomingDeadlinesAsync(int days, bool includeRelations = false);
    }
}
