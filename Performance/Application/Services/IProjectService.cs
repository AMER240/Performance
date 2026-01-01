using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Infrastructure.Entities;

namespace Performance.Application.Services
{
    public interface IProjectService
    {
        // Basic CRUD
        Task<List<ProjectEntity>> ListAsync(bool includeTasks = false);
        Task<ProjectEntity?> GetAsync(int id, bool includeTasks = false);
        Task<ProjectEntity> CreateAsync(ProjectEntity project);
        Task UpdateAsync(ProjectEntity project);
        Task DeleteAsync(int id);
        
        // Pagination
        Task<(List<ProjectEntity> Projects, int TotalCount)> ListPagedAsync(int pageNumber, int pageSize, bool includeTasks = false);
        
        // Filtered queries
        Task<List<ProjectEntity>> ListByStatusAsync(ProjectStatus status, bool includeTasks = false);
        Task<List<ProjectEntity>> ListByUserAsync(string userId, bool includeTasks = false);
        Task<int> GetTaskCountAsync(int projectId);
    }
}
