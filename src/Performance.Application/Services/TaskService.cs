using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Performance.Domain.Entities;
using TaskStatus = Performance.Domain.Enums.TaskStatus;
using TaskPriority = Performance.Domain.Enums.TaskPriority;
using Performance.Application.Interfaces;

namespace Performance.Application.Services
{
    /// <summary>
    /// Task service implementation
    /// Clean Architecture compliant - uses repositories instead of direct DbContext
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<List<TaskEntity>> ListAsync(bool includeRelations = false)
        {
            var tasks = await _taskRepository.GetAllAsync();
            return tasks.OrderByDescending(t => t.CreatedAt).ToList();
        }

        public async Task<List<TaskEntity>> ListByProjectAsync(int projectId, bool includeRelations = false)
        {
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);
            return tasks.OrderBy(t => t.Status).ThenByDescending(t => t.Priority).ToList();
        }
        
        public async Task<List<TaskEntity>> ListByUserAsync(string userId, bool includeRelations = false)
        {
            var tasks = await _taskRepository.GetTasksByUserIdAsync(userId);
            return tasks.OrderBy(t => t.Status).ThenBy(t => t.Deadline).ToList();
        }

        public async Task<TaskEntity?> GetAsync(int id, bool includeRelations = true)
        {
            return await _taskRepository.GetByIdAsync(id);
        }

        public async Task<TaskEntity> CreateAsync(TaskEntity task)
        {
            task.CreatedAt = DateTime.UtcNow;
            return await _taskRepository.AddAsync(task);
        }

        public async Task UpdateAsync(TaskEntity task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task);
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task != null) 
            { 
                await _taskRepository.DeleteAsync(task);
            }
        }
        
        public async Task SoftDeleteAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task != null)
            {
                task.IsDeleted = true;
                task.DeletedAt = DateTime.UtcNow;
                await _taskRepository.UpdateAsync(task);
            }
        }
        
        public async Task<(List<TaskEntity> Tasks, int TotalCount)> ListPagedAsync(int pageNumber, int pageSize, bool includeRelations = false)
        {
            var allTasks = await ListAsync(includeRelations);
            var totalCount = allTasks.Count;
            
            var tasks = allTasks
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            return (tasks, totalCount);
        }
        
        public async Task<List<TaskEntity>> ListByStatusAsync(TaskStatus status, bool includeRelations = false)
        {
            var tasks = await _taskRepository.GetTasksByStatusAsync(status);
            return tasks.OrderByDescending(t => t.Priority).ToList();
        }
        
        public async Task<List<TaskEntity>> ListByPriorityAsync(TaskPriority priority, bool includeRelations = false)
        {
            var allTasks = await _taskRepository.FindAsync(t => t.Priority == priority);
            return allTasks.OrderBy(t => t.Deadline).ToList();
        }
        
        public async Task<List<TaskEntity>> ListUpcomingDeadlinesAsync(int days, bool includeRelations = false)
        {
            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(days);
            
            var allTasks = await _taskRepository.FindAsync(t => 
                t.Deadline != null && 
                t.Deadline >= today && 
                t.Deadline <= endDate && 
                t.Status != TaskStatus.Done);
            
            return allTasks.OrderBy(t => t.Deadline).ToList();
        }
    }
}
