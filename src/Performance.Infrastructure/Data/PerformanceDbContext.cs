using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using TaskStatus = Performance.Domain.Enums.TaskStatus;

namespace Performance.Infrastructure.Data
{
    /// <summary>
    /// Database context for Performance application
    /// Clean Architecture compliant - located in Infrastructure layer
    /// </summary>
    public class PerformanceDbContext : DbContext
    {
        public PerformanceDbContext(DbContextOptions<PerformanceDbContext> options) : base(options) { }
        
        public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
        public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
        public DbSet<UserEntity> Users => Set<UserEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // ====== USER ENTITY CONFIGURATION ======
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(u => u.Id);
                
                // Unique constraint on UserName
                entity.HasIndex(u => u.UserName).IsUnique();
                
                // Indexes for faster queries
                entity.HasIndex(u => u.Sector);
                entity.HasIndex(u => u.Role);
                
                // String length limits for better performance
                entity.Property(u => u.UserName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(255);
                entity.Property(u => u.FullName).HasMaxLength(200);
                entity.Property(u => u.Sector).HasMaxLength(100);
                
                // Enum conversion
                entity.Property(u => u.Role).HasConversion<string>();
                
                // Relationships
                entity.HasMany(u => u.AssignedTasks)
                      .WithOne(t => t.AssignedToUser)
                      .HasForeignKey(t => t.AssignedToUserId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasMany(u => u.ManagedProjects)
                      .WithOne(p => p.CreatedBy)
                      .HasForeignKey(p => p.CreatedById)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            
            // ====== PROJECT ENTITY CONFIGURATION ======
            modelBuilder.Entity<ProjectEntity>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                // Indexes
                entity.HasIndex(p => p.CreatedAt);
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.CreatedById);
                
                // String length limits
                entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
                
                // Enum conversion
                entity.Property(p => p.Status).HasConversion<string>().HasDefaultValue(ProjectStatus.Active);
                
                // Default values
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Relationships
                entity.HasMany(p => p.Tasks)
                      .WithOne(t => t.Project)
                      .HasForeignKey(t => t.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // ====== TASK ENTITY CONFIGURATION ======
            modelBuilder.Entity<TaskEntity>(entity =>
            {
                entity.HasKey(t => t.Id);
                
                // Single-column indexes
                entity.HasIndex(t => t.ProjectId);
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => t.Priority);
                entity.HasIndex(t => t.Deadline);
                entity.HasIndex(t => t.AssignedToUserId);
                entity.HasIndex(t => t.IsDeleted);
                
                // Composite index for common queries (Status + Priority)
                entity.HasIndex(t => new { t.Status, t.Priority });
                
                // String length limits
                entity.Property(t => t.Title).HasMaxLength(300).IsRequired();
                
                // Enum conversions
                entity.Property(t => t.Status).HasConversion<string>();
                entity.Property(t => t.Priority).HasConversion<string>();
                
                // Default values
                entity.Property(t => t.IsDeleted).HasDefaultValue(false);
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Query filter for soft delete (automatically excludes deleted tasks)
                entity.HasQueryFilter(t => !t.IsDeleted);
            });
        }
        
        // Override SaveChanges to auto-update timestamps
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }
        
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is TaskEntity && (e.State == EntityState.Modified));
            
            foreach (var entry in entries)
            {
                if (entry.Entity is TaskEntity task)
                {
                    task.UpdatedAt = DateTime.UtcNow;
                    
                    // Auto-set CompletedAt when status changes to Done
                    if (task.Status == TaskStatus.Done && task.CompletedAt == null)
                    {
                        task.CompletedAt = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
