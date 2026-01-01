using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Performance.Infrastructure.Application;
using Performance.Infrastructure.Entities;

namespace Performance.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly PerformanceDbContext _db;

        public ProjectService(PerformanceDbContext db)
        {
            _db = db;
        }

        // ====== BASIC CRUD ======
        
        public async Task<List<ProjectEntity>> ListAsync(bool includeTasks = false)
        {
            var query = _db.Projects.AsNoTracking(); // ? No tracking for read-only
            
            if (includeTasks)
            {
                // Only include tasks, not full eager loading
                query = query.Include(p => p.Tasks.Where(t => !t.IsDeleted));
            }
            
            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<ProjectEntity?> GetAsync(int id, bool includeTasks = false)
        {
            var query = _db.Projects.AsQueryable();
            
            if (includeTasks)
            {
                query = query.Include(p => p.Tasks.Where(t => !t.IsDeleted));
            }
            
            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProjectEntity> CreateAsync(ProjectEntity project)
        {
            project.CreatedAt = DateTime.UtcNow;
            project.Status = ProjectStatus.Active;
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }

        public async Task UpdateAsync(ProjectEntity project)
        {
            _db.Projects.Update(project);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.Projects.FindAsync(id);
            if (p != null)
            {
                _db.Projects.Remove(p);
                await _db.SaveChangesAsync();
            }
        }
        
        // ====== PAGINATION ======
        
        public async Task<(List<ProjectEntity> Projects, int TotalCount)> ListPagedAsync(int pageNumber, int pageSize, bool includeTasks = false)
        {
            var query = _db.Projects.AsNoTracking();
            
            var totalCount = await query.CountAsync();
            
            if (includeTasks)
            {
                query = query.Include(p => p.Tasks.Where(t => !t.IsDeleted));
            }
            
            var projects = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return (projects, totalCount);
        }
        
        // ====== FILTERED QUERIES ======
        
        public async Task<List<ProjectEntity>> ListByStatusAsync(ProjectStatus status, bool includeTasks = false)
        {
            var query = _db.Projects
                .AsNoTracking()
                .Where(p => p.Status == status);
            
            if (includeTasks)
            {
                query = query.Include(p => p.Tasks.Where(t => !t.IsDeleted));
            }
            
            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
        
        public async Task<List<ProjectEntity>> ListByUserAsync(string userId, bool includeTasks = false)
        {
            var query = _db.Projects
                .AsNoTracking()
                .Where(p => p.CreatedById == userId);
            
            if (includeTasks)
            {
                query = query.Include(p => p.Tasks.Where(t => !t.IsDeleted));
            }
            
            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
        
        public async Task<int> GetTaskCountAsync(int projectId)
        {
            // Optimized count query without loading entities
            return await _db.Tasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .CountAsync();
        }
    }
}
