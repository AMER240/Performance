using System;
using System.Linq;
using System.Threading.Tasks;
using Performance.Infrastructure.Application;
using Performance.Infrastructure.Entities;

namespace Performance.Application.Services
{
    public class RuleBasedTaskSuggestionService : ITaskSuggestionService
    {
        private readonly PerformanceDbContext _db;

        public RuleBasedTaskSuggestionService(PerformanceDbContext db)
        {
            _db = db;
        }

        public async Task<SuggestionResult> SuggestAsync(string taskDescription, int? projectId = null)
        {
            var desc = (taskDescription ?? string.Empty).ToLowerInvariant();
            var priority = TaskPriority.Medium;
            var estimate = TimeSpan.FromHours(4);
            var reasons = new System.Collections.Generic.List<string>();

            if (desc.Contains("urgent") || desc.Contains("asap") || desc.Contains("critical"))
            {
                priority = TaskPriority.High;
                estimate = TimeSpan.FromHours(8);
                reasons.Add("Contains urgent keywords");
            }
            else if (desc.Contains("research") || desc.Contains("investigate") || desc.Contains("study"))
            {
                priority = TaskPriority.Low;
                estimate = TimeSpan.FromHours(24);
                reasons.Add("Research type task detected");
            }

            string requiredRole = desc.Contains("manage") || desc.Contains("plan") ? "Manager" : "Employee";
            var candidates = _db.Users.Where(u => u.Role == (requiredRole == "Manager" ? UserRole.Manager : UserRole.Employee)).Take(5).Select(u => u.Id).ToArray();

            if (!candidates.Any())
            {
                candidates = _db.Users.Take(3).Select(u => u.Id).ToArray();
                reasons.Add("No direct role match found, falling back to any user");
            }

            var explanation = string.Join("; ", reasons);
            return await Task.FromResult(new SuggestionResult(candidates, priority, estimate, explanation));
        }
    }
}
