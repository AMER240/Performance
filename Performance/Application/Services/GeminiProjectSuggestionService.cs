using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Performance.Infrastructure.Application;

namespace Performance.Application.Services
{
    /// <summary>
    /// AI-powered project suggestion service using Google Gemini API
    /// </summary>
    public class GeminiProjectSuggestionService : IProjectSuggestionService
    {
        private readonly PerformanceDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiProjectSuggestionService(PerformanceDbContext db, IConfiguration configuration, HttpClient httpClient)
        {
            _db = db;
            _configuration = configuration;
            _httpClient = httpClient;
            _apiKey = _configuration["GeminiAI:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
            _model = _configuration["GeminiAI:Model"] ?? "gemini-1.5-flash";
        }

        public async Task<ProjectSuggestionResult> SuggestProjectDetailsAsync(string projectName, string projectDescription)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                return CreateDefaultSuggestion("No project name provided");
            }

            try
            {
                // Get available users and their sectors for context
                var users = await _db.Users.ToListAsync();
                var userContext = new StringBuilder();
                userContext.AppendLine("Available team members:");
                foreach (var user in users)
                {
                    userContext.AppendLine($"- {user.UserName} ({user.Role})");
                }

                // Get historical projects for context
                var recentProjects = await _db.Projects
                    .Include(p => p.Tasks)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                var projectContext = new StringBuilder();
                if (recentProjects.Any())
                {
                    projectContext.AppendLine("\nRecent projects for reference:");
                    foreach (var proj in recentProjects)
                    {
                        projectContext.AppendLine($"- {proj.Name}: {proj.Tasks.Count} tasks");
                    }
                }

                // Build prompt
                var prompt = BuildPrompt(projectName, projectDescription, userContext.ToString(), projectContext.ToString());

                // Call Gemini API
                var geminiResponse = await CallGeminiApi(prompt);

                // Parse response
                var suggestion = ParseGeminiResponse(geminiResponse);

                return suggestion;
            }
            catch (Exception ex)
            {
                return CreateDefaultSuggestion($"API Error: {ex.Message}");
            }
        }

        private string BuildPrompt(string projectName, string projectDescription, string userContext, string projectContext)
        {
            var desc = !string.IsNullOrWhiteSpace(projectDescription) ? projectDescription : "No description provided";

            var prompt = $@"You are an AI project management consultant. Analyze the following project and provide detailed suggestions.

PROJECT NAME: {projectName}
PROJECT DESCRIPTION: {desc}

{userContext}
{projectContext}

Based on the project information, provide:
1. **Suggested Features**: List 5-7 key features this project should have
2. **Recommended Tasks**: List 5-8 initial tasks to get started
3. **Required Employee Types**: What kind of roles/specialists are needed (e.g., Backend Developer, UI Designer, QA Tester)
4. **Team Composition**: Recommended team size and structure
5. **Explanation**: Brief overview of the project approach

IMPORTANT: Respond ONLY in this exact JSON format (no additional text):
{{
  ""suggestedFeatures"": ""Feature 1\nFeature 2\nFeature 3..."",
  ""recommendedTasks"": ""Task 1\nTask 2\nTask 3..."",
  ""requiredEmployeeTypes"": ""Role 1\nRole 2\nRole 3..."",
  ""teamComposition"": ""Team structure description"",
  ""explanation"": ""Your overall recommendation""
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

        private ProjectSuggestionResult ParseGeminiResponse(string geminiResponse)
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

                    var features = root.GetProperty("suggestedFeatures").GetString() ?? "";
                    var tasks = root.GetProperty("recommendedTasks").GetString() ?? "";
                    var employeeTypes = root.GetProperty("requiredEmployeeTypes").GetString() ?? "";
                    var teamComp = root.GetProperty("teamComposition").GetString() ?? "";
                    var explanation = root.GetProperty("explanation").GetString() ?? "";

                    return new ProjectSuggestionResult(features, tasks, employeeTypes, teamComp, $"? Gemini AI: {explanation}");
                }
            }
            catch
            {
                // Parsing failed
            }

            return CreateDefaultSuggestion("Failed to parse Gemini response");
        }

        private ProjectSuggestionResult CreateDefaultSuggestion(string reason)
        {
            return new ProjectSuggestionResult(
                "- Define project scope\n- Setup development environment\n- Create initial documentation",
                "- Project kickoff meeting\n- Requirements gathering\n- Technical architecture design\n- Setup version control",
                "- Project Manager\n- Software Developer\n- Quality Assurance\n- Designer",
                "Small team: 3-5 members with diverse skills",
                reason
            );
        }
    }
}
