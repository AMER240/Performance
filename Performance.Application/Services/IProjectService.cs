using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Infrastructure.Entities;

namespace Performance.Application.Services
{
    public interface IProjectService
    {
        Task<List<ProjectEntity>> ListAsync();
        Task<ProjectEntity?> GetAsync(int id);
        Task<ProjectEntity> CreateAsync(ProjectEntity project);
        Task UpdateAsync(ProjectEntity project);
        Task DeleteAsync(int id);
    }
}
