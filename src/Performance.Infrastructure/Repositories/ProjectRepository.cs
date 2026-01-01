using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.Infrastructure.Data;

namespace Performance.Infrastructure.Repositories
{
    /// <summary>
    /// Project repository implementation
    /// </summary>
    public class ProjectRepository : Repository<ProjectEntity>, IProjectRepository
    {
        public ProjectRepository(PerformanceDbContext context) : base(context)
        {
        }

        public async Task<List<ProjectEntity>> GetProjectsByStatusAsync(ProjectStatus status)
        {
            return await _dbSet
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProjectEntity>> SearchProjectsByNameAsync(string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            
            return await _dbSet
                .Where(p => p.Name.ToLower().Contains(lowerSearchTerm) || 
                           (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm)))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProjectEntity?> GetProjectWithTasksAsync(int projectId)
        {
            return await _dbSet
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }
    }
}
