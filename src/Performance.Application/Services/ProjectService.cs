using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.Application.Interfaces;

namespace Performance.Application.Services
{
    /// <summary>
    /// Project service implementation
    /// Clean Architecture compliant - uses repositories instead of direct DbContext
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;

        public ProjectService(IProjectRepository projectRepository, ITaskRepository taskRepository)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
        }

        public async Task<List<ProjectEntity>> ListAsync(bool includeTasks = false)
        {
            if (includeTasks)
            {
                var projectsWithTasks = new List<ProjectEntity>();
                var allProjects = await _projectRepository.GetAllAsync();
                
                foreach (var project in allProjects.OrderByDescending(p => p.CreatedAt))
                {
                    var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
                    project.Tasks = tasks;
                    projectsWithTasks.Add(project);
                }
                
                return projectsWithTasks;
            }
            
            var projects = await _projectRepository.GetAllAsync();
            return projects.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<ProjectEntity?> GetAsync(int id, bool includeTasks = false)
        {
            if (includeTasks)
            {
                return await _projectRepository.GetProjectWithTasksAsync(id);
            }
            
            return await _projectRepository.GetByIdAsync(id);
        }

        public async Task<ProjectEntity> CreateAsync(ProjectEntity project)
        {
            project.CreatedAt = DateTime.UtcNow;
            project.Status = ProjectStatus.Active;
            return await _projectRepository.AddAsync(project);
        }

        public async Task UpdateAsync(ProjectEntity project)
        {
            await _projectRepository.UpdateAsync(project);
        }

        public async Task DeleteAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project != null)
            {
                await _projectRepository.DeleteAsync(project);
            }
        }
        
        public async Task<(List<ProjectEntity> Projects, int TotalCount)> ListPagedAsync(int pageNumber, int pageSize, bool includeTasks = false)
        {
            var allProjects = await ListAsync(includeTasks);
            var totalCount = allProjects.Count;
            
            var projects = allProjects
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            return (projects, totalCount);
        }
        
        public async Task<List<ProjectEntity>> ListByStatusAsync(ProjectStatus status, bool includeTasks = false)
        {
            var projects = await _projectRepository.GetProjectsByStatusAsync(status);
            
            if (includeTasks)
            {
                foreach (var project in projects)
                {
                    var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
                    project.Tasks = tasks;
                }
            }
            
            return projects;
        }
        
        public async Task<List<ProjectEntity>> ListByUserAsync(string userId, bool includeTasks = false)
        {
            var projects = await _projectRepository.FindAsync(p => p.CreatedById == userId);
            var projectList = projects.OrderByDescending(p => p.CreatedAt).ToList();
            
            if (includeTasks)
            {
                foreach (var project in projectList)
                {
                    var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
                    project.Tasks = tasks;
                }
            }
            
            return projectList;
        }
        
        public async Task<int> GetTaskCountAsync(int projectId)
        {
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);
            return tasks.Count;
        }
    }
}
