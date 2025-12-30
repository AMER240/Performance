using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Infrastructure.Entities;

namespace Performance.Application.Services
{
    public interface ITaskService
    {
        Task<List<TaskEntity>> ListAsync(); // Get all tasks
        Task<List<TaskEntity>> ListByProjectAsync(int projectId);
        Task<TaskEntity?> GetAsync(int id);
        Task<TaskEntity> CreateAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task DeleteAsync(int id);
    }
}
