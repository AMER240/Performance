using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using TaskStatus = Performance.Domain.Enums.TaskStatus;
using Performance.Infrastructure.Data;

namespace Performance.Infrastructure.Repositories
{
    /// <summary>
    /// Task repository implementation with domain-specific queries
    /// </summary>
    public class TaskRepository : Repository<TaskEntity>, ITaskRepository
    {
        public TaskRepository(PerformanceDbContext context) : base(context)
        {
        }

        public override async Task<TaskEntity?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Project)
                    .Include(t => t.AssignedToUser)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<TaskEntity>> GetCompletedTasksAsync(int? projectId = null, int maxCount = 100)
        {
            var query = _dbSet
                .Include(t => t.Project)
                .Where(t => t.Status == TaskStatus.Done);

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            return await query
                .OrderByDescending(t => t.Id)
                .Take(maxCount)
                .ToListAsync();
        }

        public async Task<List<TaskEntity>> GetTasksByProjectIdAsync(int projectId)
        {
            return await _dbSet
                .Where(t => t.ProjectId == projectId && !t.IsDeleted)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TaskEntity>> GetTasksByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(t => t.AssignedToUserId == userId && !t.IsDeleted)
                .Include(t => t.Project)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<string>> GetActiveUsersInProjectAsync(int projectId, int maxCount = 3)
        {
            return await _dbSet
                .Where(t => t.ProjectId == projectId && !string.IsNullOrEmpty(t.AssignedToUserId))
                .GroupBy(t => t.AssignedToUserId)
                .OrderByDescending(g => g.Count())
                .Take(maxCount)
                .Select(g => g.Key!)
                .ToListAsync();
        }

        public async Task<List<TaskEntity>> GetTasksByStatusAsync(TaskStatus status)
        {
            return await _dbSet
                .Where(t => t.Status == status && !t.IsDeleted)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
