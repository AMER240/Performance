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
    /// Enhanced AI-powered task suggestion service using historical data analysis
    /// Clean Architecture compliant - uses repositories instead of direct DbContext
    /// </summary>
    public class EnhancedTaskSuggestionService : ITaskSuggestionService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private const int MinSimilarityThreshold = 30; // %30 benzerlik e?i?i
        private const int MaxHistoricalTasks = 100; // Son 100 görevi analiz et

        public EnhancedTaskSuggestionService(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public async Task<SuggestionResult> SuggestAsync(string taskDescription, int? projectId = null)
        {
            var desc = (taskDescription ?? string.Empty).ToLowerInvariant().Trim();
            if (string.IsNullOrWhiteSpace(desc))
            {
                return await CreateDefaultSuggestion("No description provided");
            }

            var reasons = new List<string>();

            // 1. Geçmi? görevleri analiz et
            var historicalTasks = await GetHistoricalTasks(projectId);
            
            // 2. Benzer görevleri bul
            var similarTasks = FindSimilarTasks(desc, historicalTasks);
            
            // 3. Öncelik öner
            var priority = SuggestPriority(desc, similarTasks, out var priorityReason);
            reasons.Add(priorityReason);

            // 4. Süre tahmini
            var estimate = EstimateDuration(desc, similarTasks, out var durationReason);
            reasons.Add(durationReason);

            // 5. Kullan?c? önerisi
            var suggestedUserIds = await SuggestUsers(desc, similarTasks, projectId);
            reasons.Add(suggestedUserIds.reason);

            var explanation = string.Join(" | ", reasons);
            return new SuggestionResult(suggestedUserIds.userIds.ToArray(), priority, estimate, explanation);
        }

        private async Task<List<TaskEntity>> GetHistoricalTasks(int? projectId)
        {
            return await _taskRepository.GetCompletedTasksAsync(projectId, MaxHistoricalTasks);
        }

        private List<SimilarTask> FindSimilarTasks(string description, List<TaskEntity> historicalTasks)
        {
            var similarTasks = new List<SimilarTask>();

            foreach (var task in historicalTasks)
            {
                var taskDesc = (task.Description ?? string.Empty).ToLowerInvariant();
                var taskTitle = (task.Title ?? string.Empty).ToLowerInvariant();
                
                // Basit kelime bazl? benzerlik skoru
                var similarity = CalculateSimilarity(description, taskDesc + " " + taskTitle);
                
                if (similarity >= MinSimilarityThreshold)
                {
                    similarTasks.Add(new SimilarTask
                    {
                        Task = task,
                        SimilarityScore = similarity
                    });
                }
            }

            return similarTasks.OrderByDescending(st => st.SimilarityScore).Take(10).ToList();
        }

        private int CalculateSimilarity(string text1, string text2)
        {
            // Basit kelime bazl? benzerlik (Jaccard similarity)
            var words1 = text1.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(w => w.Length > 2) // 2 karakterden uzun kelimeler
                             .Distinct()
                             .ToHashSet();

            var words2 = text2.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(w => w.Length > 2)
                             .Distinct()
                             .ToHashSet();

            if (!words1.Any() || !words2.Any()) return 0;

            var intersection = words1.Intersect(words2).Count();
            var union = words1.Union(words2).Count();

            return (int)((intersection / (double)union) * 100);
        }

        private TaskPriority SuggestPriority(string description, List<SimilarTask> similarTasks, out string reason)
        {
            // 1. Keyword bazl? aciliyet kontrolü (legacy)
            if (description.Contains("urgent") || description.Contains("asap") || description.Contains("critical") || description.Contains("emergency"))
            {
                reason = "Urgent keywords detected";
                return TaskPriority.High;
            }

            if (description.Contains("low priority") || description.Contains("when possible") || description.Contains("someday"))
            {
                reason = "Low priority keywords detected";
                return TaskPriority.Low;
            }

            // 2. Benzer görevlerin ortalama önceli?i
            if (similarTasks.Any())
            {
                var avgPriority = similarTasks.Average(st => (int)st.Task.Priority);
                var priority = avgPriority switch
                {
                    >= 2.5 => TaskPriority.High,
                    >= 1.5 => TaskPriority.Medium,
                    _ => TaskPriority.Low
                };

                var topMatch = similarTasks.First();
                reason = $"Based on {similarTasks.Count} similar task(s) (best match: {topMatch.SimilarityScore}% similar)";
                return priority;
            }

            // 3. Görev tipi analizi
            if (description.Contains("research") || description.Contains("investigate") || description.Contains("study"))
            {
                reason = "Research/investigation type task";
                return TaskPriority.Low;
            }

            if (description.Contains("bug") || description.Contains("fix") || description.Contains("error"))
            {
                reason = "Bug fix task detected";
                return TaskPriority.High;
            }

            reason = "Default medium priority (no historical data)";
            return TaskPriority.Medium;
        }

        private TimeSpan EstimateDuration(string description, List<SimilarTask> similarTasks, out string reason)
        {
            // 1. Benzer görevlerin ortalama süresi
            if (similarTasks.Any())
            {
                var tasksWithDuration = similarTasks
                    .Where(st => st.Task.EstimatedDuration.HasValue)
                    .ToList();

                if (tasksWithDuration.Any())
                {
                    var avgHours = tasksWithDuration.Average(st => st.Task.EstimatedDuration!.Value.TotalHours);
                    reason = $"Average of {tasksWithDuration.Count} similar completed task(s): {avgHours:F1}h";
                    return TimeSpan.FromHours(Math.Max(1, avgHours)); // En az 1 saat
                }
            }

            // 2. Kelime say?s?na göre tahmin (basit)
            var wordCount = description.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var estimatedHours = wordCount switch
            {
                < 5 => 2,   // K?sa aç?klama: 2 saat
                < 15 => 4,  // Orta aç?klama: 4 saat
                < 30 => 8,  // Uzun aç?klama: 8 saat
                _ => 16     // Çok uzun: 16 saat
            };

            // 3. Görev tipi bazl? ayarlama
            if (description.Contains("research") || description.Contains("investigate"))
            {
                estimatedHours *= 2; // Ara?t?rma görevleri daha uzun
                reason = $"Research task estimation: {estimatedHours}h";
            }
            else if (description.Contains("quick") || description.Contains("simple") || description.Contains("small"))
            {
                estimatedHours = Math.Max(1, estimatedHours / 2); // Basit görevler daha k?sa
                reason = $"Quick task estimation: {estimatedHours}h";
            }
            else
            {
                reason = $"Word count based estimation: {estimatedHours}h";
            }

            return TimeSpan.FromHours(estimatedHours);
        }

        private async Task<(List<string> userIds, string reason)> SuggestUsers(string description, List<SimilarTask> similarTasks, int? projectId)
        {
            var suggestedUserIds = new List<string>();

            // 1. Benzer görevleri tamamlayan kullan?c?lar
            if (similarTasks.Any())
            {
                var userAssignments = similarTasks
                    .Where(st => !string.IsNullOrEmpty(st.Task.AssignedToUserId))
                    .GroupBy(st => st.Task.AssignedToUserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        Count = g.Count(),
                        AvgSimilarity = g.Average(st => st.SimilarityScore)
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenByDescending(x => x.AvgSimilarity)
                    .Take(3)
                    .ToList();

                if (userAssignments.Any())
                {
                    var userIds = userAssignments.Select(ua => ua.UserId!).ToList();

                    if (userIds.Any())
                    {
                        suggestedUserIds.AddRange(userIds);
                        var topUser = userAssignments.First();
                        return (suggestedUserIds, $"User(s) who completed {topUser.Count} similar task(s)");
                    }
                }
            }

            // 2. Proje bazl? öneri (ayn? projede aktif kullan?c?lar)
            if (projectId.HasValue)
            {
                var projectUsers = await _taskRepository.GetActiveUsersInProjectAsync(projectId.Value, 3);

                if (projectUsers.Any())
                {
                    suggestedUserIds.AddRange(projectUsers);
                    return (suggestedUserIds, $"Active user(s) in this project");
                }
            }

            // 3. Role bazl? öneri (legacy)
            var requiredRole = description.Contains("manage") || description.Contains("plan") || description.Contains("coordinate")
                ? UserRole.Manager
                : UserRole.Employee;

            var roleUsers = await _userRepository.GetUsersByRoleAsync(requiredRole, 3);

            if (roleUsers.Any())
            {
                suggestedUserIds.AddRange(roleUsers.Select(u => u.Id));
                return (suggestedUserIds, $"Users with {requiredRole} role");
            }

            // 4. Fallback: herhangi bir kullan?c?
            var allUsers = await _userRepository.GetAllUsersAsync();
            var anyUsers = allUsers.Take(3).Select(u => u.Id).ToList();
            suggestedUserIds.AddRange(anyUsers);
            return (suggestedUserIds, "Default user assignment (no historical data)");
        }

        private async Task<SuggestionResult> CreateDefaultSuggestion(string reason)
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            var defaultUsers = allUsers.Take(3).Select(u => u.Id).ToArray();
            return new SuggestionResult(
                defaultUsers,
                TaskPriority.Medium,
                TimeSpan.FromHours(4),
                reason
            );
        }

        private class SimilarTask
        {
            public TaskEntity Task { get; set; } = null!;
            public int SimilarityScore { get; set; }
        }
    }
}
