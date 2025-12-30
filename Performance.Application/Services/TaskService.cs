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

        public async Task<List<TaskEntity>> ListAsync()
        {
            return await _db.Tasks.ToListAsync();
        }

        public async Task<List<TaskEntity>> ListByProjectAsync(int projectId)
        {
            return await _db.Tasks.Where(t => t.ProjectId == projectId).ToListAsync();
        }

        public async Task<TaskEntity?> GetAsync(int id)
        {
            return await _db.Tasks.FindAsync(id);
        }

        public async Task<TaskEntity> CreateAsync(TaskEntity task)
        {
            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            return task;
        }

        public async Task UpdateAsync(TaskEntity task)
        {
            _db.Tasks.Update(task);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var t = await _db.Tasks.FindAsync(id);
            if (t != null) { _db.Tasks.Remove(t); await _db.SaveChangesAsync(); }
        }
    }
}
