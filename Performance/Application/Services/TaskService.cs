using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Performance.Infrastructure.Application;
using Performance.Infrastructure.Entities;

namespace Performance.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly PerformanceDbContext _db;

        public TaskService(PerformanceDbContext db)
        {
            _db = db;
        }

        // ====== BASIC CRUD ======
        
        public async Task<List<TaskEntity>> ListAsync(bool includeRelations = false)
        {
            var query = _db.Tasks.AsNoTracking(); // ? No tracking for read-only
            
            if (includeRelations)
            {
                query = query
                    .Include(t => t.Project)
                    .Include(t => t.AssignedToUser);
            }
            
            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<List<TaskEntity>> ListByProjectAsync(int projectId, bool includeRelations = false)
        {
            var query = _db.Tasks
                .IgnoreQueryFilters() // ? GEÇICI FIX: Query filter'? devre d??? b?rak
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId);
            
            if (includeRelations)
            {
                query = query
                    .Include(t => t.Project)
                    .Include(t => t.AssignedToUser);
            }
            
            return await query.OrderBy(t => t.Status).ThenByDescending(t => t.Priority).ToListAsync();
        }
        
        public async Task<List<TaskEntity>> ListByUserAsync(string userId, bool includeRelations = false)
        {
            var query = _db.Tasks
                .AsNoTracking()
                .Where(t => t.AssignedToUserId == userId);
            
            if (includeRelations)
            {
                query = query
                    .Include(t => t.Project)
                    .Include(t => t.AssignedToUser);
            }
            
            return await query.OrderBy(t => t.Status).ThenBy(t => t.Deadline).ToListAsync();
        }

        public async Task<TaskEntity?> GetAsync(int id, bool includeRelations = true)
        {
            var query = _db.Tasks.AsQueryable();
            
            if (includeRelations)
            {
                query = query
                    .Include(t => t.Project)
                    .Include(t => t.AssignedToUser);
            }
            
            return await query.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TaskEntity> CreateAsync(TaskEntity task)
        {
            task.CreatedAt = DateTime.UtcNow;
            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            return task;
        }

        public async Task UpdateAsync(TaskEntity task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _db.Tasks.Update(task);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var t = await _db.Tasks.FindAsync(id);
            if (t != null) 
            { 
                _db.Tasks.Remove(t); 
                await _db.SaveChangesAsync(); 
            }
        }
        
        public async Task SoftDeleteAsync(int id)
        {
            var t = await _db.Tasks.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id);
            if (t != null)
            {
                t.IsDeleted = true;
                t.DeletedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
        
        // ====== PAGINATION ======
        
        public async Task<(List<TaskEntity> Tasks, int TotalCount)> ListPagedAsync(int pageNumber, int pageSize, bool includeRelations = false)
        {
            var query = _db.Tasks.AsNoTracking();
            
            if (includeRelations)
            {
                query = query
                    .Include(t => t.Project)
                    .Include(t => t.AssignedToUser);
            }
            
            var totalCount = await query.CountAsync();
            
            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return (tasks, totalCount);
        }
        
        // ====== FILTERED QUERIES ======
        
        public async Task<List<TaskEntity>> ListByStatusAsync(Infrastructure.Entities.TaskStatus status, bool includeRelations = false)
        {
            var query = _db.Tasks
                .AsNoTracking()
                .Where(t => t.Status == status);
            
            if (includeRelations)
            {
                query = query
                    .Include(t => t.Project)
                    .Include(t => t.AssignedToUser);
            }
            
            return await query.OrderByDescending(t => t.Priority).ToListAsync();
        }
        
        public async Task<List<TaskEntity>> ListByPriorityAsync(Infrastructure.Entities.TaskPriority priority, bool includeRelations = false)
        {
            var query = _db.Tasks
                .AsNoTracking()
                .Where(t => t.Priority == priority);
            
            if (includeRelations)
            {
                query = query
                    .Include(t => t.Project)
                    .Include(t => t.AssignedToUser);
            }
            
            return await query.OrderBy(t => t.Deadline).ToListAsync();
        }
        
        public async Task<List<TaskEntity>> ListUpcomingDeadlinesAsync(int days, bool includeRelations = false)
        {
            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(days);
            
            var query = _db.Tasks
                .AsNoTracking()
                .Where(t => t.Deadline != null && t.Deadline >= today && t.Deadline <= endDate && t.Status != Infrastructure.Entities.TaskStatus.Done);
            
            if (includeRelations)
            {
                query = query
                    .Include(t => t.Project)
                    .Include(t => t.AssignedToUser);
            }
            
            return await query.OrderBy(t => t.Deadline).ToListAsync();
        }
    }
}
