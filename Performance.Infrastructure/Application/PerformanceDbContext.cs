using Microsoft.EntityFrameworkCore;
using Performance.Infrastructure.Entities;

namespace Performance.Infrastructure.Application
{
    public class PerformanceDbContext : DbContext
    {
        public PerformanceDbContext(DbContextOptions<PerformanceDbContext> options) : base(options) { }
        public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
        public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
        public DbSet<UserEntity> Users => Set<UserEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserEntity>().HasKey(u => u.Id);
            modelBuilder.Entity<ProjectEntity>().HasMany(p => p.Tasks).WithOne(t => t.Project).HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TaskEntity>().Property(t => t.Status).HasConversion<string>();
            modelBuilder.Entity<TaskEntity>().Property(t => t.Priority).HasConversion<string>();
        }
    }
}