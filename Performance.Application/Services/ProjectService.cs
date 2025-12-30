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

        public async Task<List<ProjectEntity>> ListAsync()
        {
            return await _db.Projects.Include(p => p.Tasks).ToListAsync();
        }

        public async Task<ProjectEntity?> GetAsync(int id)
        {
            return await _db.Projects.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProjectEntity> CreateAsync(ProjectEntity project)
        {
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
    }
}
