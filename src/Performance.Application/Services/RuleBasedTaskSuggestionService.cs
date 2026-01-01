using System;
using System.Linq;
using System.Threading.Tasks;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.Application.Interfaces;

namespace Performance.Application.Services
{
    /// <summary>
    /// Rule-based task suggestion service
    /// Clean Architecture compliant - uses repositories instead of direct DbContext
    /// </summary>
    public class RuleBasedTaskSuggestionService : ITaskSuggestionService
    {
        private readonly IUserRepository _userRepository;

        public RuleBasedTaskSuggestionService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
            var roleEnum = requiredRole == "Manager" ? UserRole.Manager : UserRole.Employee;
            var roleUsers = await _userRepository.GetUsersByRoleAsync(roleEnum, 5);
            var candidates = roleUsers.Select(u => u.Id).ToArray();

            if (!candidates.Any())
            {
                var allUsers = await _userRepository.GetAllUsersAsync();
                candidates = allUsers.Take(3).Select(u => u.Id).ToArray();
                reasons.Add("No direct role match found, falling back to any user");
            }

            var explanation = string.Join("; ", reasons);
            return new SuggestionResult(candidates, priority, estimate, explanation);
        }
    }
}
