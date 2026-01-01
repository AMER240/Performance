using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.Application.Interfaces;

namespace Performance.Application.Services
{
    /// <summary>
    /// AI-powered task suggestion service using Google Gemini API
    /// Clean Architecture compliant - uses repositories instead of direct DbContext
    /// </summary>
    public class GeminiTaskSuggestionService : ITaskSuggestionService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiTaskSuggestionService(
            ITaskRepository taskRepository, 
            IUserRepository userRepository, 
            IConfiguration configuration, 
            HttpClient httpClient)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _httpClient = httpClient;
            _apiKey = _configuration["GeminiAI:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
            _model = _configuration["GeminiAI:Model"] ?? "gemini-1.5-flash";
        }

        public async Task<SuggestionResult> SuggestAsync(string taskDescription, int? projectId = null)
        {
            var desc = (taskDescription ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(desc))
            {
                return await CreateDefaultSuggestion("No description provided");
            }

            try
            {
                // Get historical data for context
                var historicalContext = await GetHistoricalContext(projectId);
                var users = await _userRepository.GetAllUsersAsync();
                
                // Build prompt for Gemini
                var prompt = BuildPrompt(desc, historicalContext, users, projectId);
                
                // Call Gemini API
                var geminiResponse = await CallGeminiApi(prompt);
                
                // Parse response
                var suggestion = await ParseGeminiResponse(geminiResponse, users);
                
                return suggestion;
            }
            catch (Exception ex)
            {
                // Fallback to default suggestion if API fails
                return await CreateDefaultSuggestion($"API Error: {ex.Message}");
            }
        }

        private async Task<string> GetHistoricalContext(int? projectId)
        {
            var completedTasks = await _taskRepository.GetCompletedTasksAsync(projectId, 20);

            if (!completedTasks.Any())
                return "No historical data available.";

            var context = new StringBuilder();
            context.AppendLine("Historical completed tasks for reference:");
            foreach (var task in completedTasks.Take(10))
            {
                context.AppendLine($"- Title: {task.Title}");
                context.AppendLine($"  Description: {task.Description}");
                context.AppendLine($"  Priority: {task.Priority}");
                context.AppendLine($"  Duration: {task.EstimatedDuration?.TotalHours ?? 0}h");
                context.AppendLine($"  Assigned: {task.AssignedToUserId}");
                context.AppendLine();
            }

            return context.ToString();
        }

        private string BuildPrompt(string taskDescription, string historicalContext, List<UserEntity> users, int? projectId)
        {
            var userList = new StringBuilder();
            userList.AppendLine("Available users:");
            foreach (var user in users)
            {
                userList.AppendLine($"- ID: {user.Id}, Name: {user.UserName}, Role: {user.Role}");
            }

            var prompt = $@"You are an AI task management assistant. Analyze the following task and provide suggestions.

TASK DESCRIPTION:
{taskDescription}

{historicalContext}

{userList}

Based on the task description and historical data, provide:
1. **Priority** (Low, Medium, or High)
2. **Estimated Duration** (in hours, as a number)
3. **Recommended User ID** (from the user list above, pick the most suitable user based on role, sector, and task type)
4. **Explanation** (brief reason for your suggestions)

IMPORTANT: Respond ONLY in this exact JSON format (no additional text):
{{
  ""priority"": ""Medium"",
  ""estimatedHours"": 4.0,
  ""userId"": ""user-id-here"",
  ""explanation"": ""Your reasoning here""
}}";

            return prompt;
        }

        private async Task<string> CallGeminiApi(string prompt)
        {
            // Try v1 endpoint first, fallback to v1beta
            var urls = new[]
            {
                $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}",
                $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}"
            };

            Exception lastException = null;

            foreach (var url in urls)
            {
                try
                {
                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = prompt }
                                }
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(url, httpContent);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // Parse Gemini response
                        using var doc = JsonDocument.Parse(responseContent);
                        var candidates = doc.RootElement.GetProperty("candidates");
                        if (candidates.GetArrayLength() > 0)
                        {
                            var firstCandidate = candidates[0];
                            var contentProperty = firstCandidate.GetProperty("content");
                            var parts = contentProperty.GetProperty("parts");
                            if (parts.GetArrayLength() > 0)
                            {
                                var text = parts[0].GetProperty("text").GetString();
                                return text ?? string.Empty;
                            }
                        }
                    }
                    else
                    {
                        // Try to extract error message from JSON
                        string errorMsg = responseContent;
                        try
                        {
                            using var errorDoc = JsonDocument.Parse(responseContent);
                            if (errorDoc.RootElement.TryGetProperty("error", out var error))
                            {
                                if (error.TryGetProperty("message", out var message))
                                {
                                    errorMsg = message.GetString() ?? responseContent;
                                }
                            }
                        }
                        catch { }

                        lastException = new HttpRequestException($"API {response.StatusCode}: {errorMsg}");
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw lastException ?? new Exception("All Gemini API endpoints failed");
        }

        private async Task<SuggestionResult> ParseGeminiResponse(string geminiResponse, List<UserEntity> users)
        {
            try
            {
                // Extract JSON from response (in case there's markdown formatting)
                var jsonStart = geminiResponse.IndexOf('{');
                var jsonEnd = geminiResponse.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonText = geminiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    
                    using var doc = JsonDocument.Parse(jsonText);
                    var root = doc.RootElement;

                    var priorityStr = root.GetProperty("priority").GetString() ?? "Medium";
                    var priority = Enum.TryParse<TaskPriority>(priorityStr, true, out var p) ? p : TaskPriority.Medium;

                    var estimatedHours = root.GetProperty("estimatedHours").GetDouble();
                    var estimatedDuration = TimeSpan.FromHours(Math.Max(1, estimatedHours));

                    var userId = root.GetProperty("userId").GetString();
                    var explanation = root.GetProperty("explanation").GetString() ?? "AI-generated suggestion";

                    // Validate user ID
                    if (!string.IsNullOrEmpty(userId) && !users.Any(u => u.Id == userId))
                    {
                        // User not found, pick first user
                        userId = users.FirstOrDefault()?.Id;
                    }

                    var userIds = !string.IsNullOrEmpty(userId) 
                        ? new[] { userId } 
                        : users.Take(1).Select(u => u.Id).ToArray();

                    return new SuggestionResult(userIds, priority, estimatedDuration, $"? Gemini AI: {explanation}");
                }
            }
            catch
            {
                // Parsing failed, return default
            }

            return await CreateDefaultSuggestion("Failed to parse Gemini response");
        }

        private async Task<SuggestionResult> CreateDefaultSuggestion(string reason)
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            var defaultUsers = allUsers.Take(1).Select(u => u.Id).ToArray();
            return new SuggestionResult(
                defaultUsers,
                TaskPriority.Medium,
                TimeSpan.FromHours(4),
                reason
            );
        }
    }
}
