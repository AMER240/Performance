using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Performance.Application.Interfaces;

namespace Performance.Application.Services
{
    /// <summary>
    /// AI-powered project suggestion service using Google Gemini API
    /// Provides insights about project features, tasks, team composition, and requirements
    /// </summary>
    public class GeminiProjectSuggestionService : IProjectSuggestionService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiProjectSuggestionService(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _apiKey = _configuration["GeminiAI:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured in appsettings.json");
            _model = _configuration["GeminiAI:Model"] ?? "gemini-2.5-flash";
        }

        public async Task<ProjectSuggestionResult> SuggestProjectDetailsAsync(string projectName, string projectDescription)
        {
            var name = (projectName ?? string.Empty).Trim();
            var desc = (projectDescription ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(desc))
            {
                return CreateDefaultSuggestion("No project information provided");
            }

            try
            {
                var prompt = BuildProjectPrompt(name, desc);
                var geminiResponse = await CallGeminiApi(prompt);
                var suggestion = ParseGeminiResponse(geminiResponse);
                return suggestion;
            }
            catch (Exception ex)
            {
                return CreateDefaultSuggestion($"API Error: {ex.Message}");
            }
        }

        private string BuildProjectPrompt(string projectName, string projectDescription)
        {
            var prompt = $@"You are an AI project management consultant. Analyze the following project and provide comprehensive insights.

PROJECT NAME: {projectName}

PROJECT DESCRIPTION: {projectDescription}

Based on the project name and description, provide detailed suggestions in the following categories:

1. **Suggested Features**: List 3-5 key features that this project should include. Be specific and practical.

2. **Recommended Tasks**: List 3-5 initial tasks to start the project. Prioritize setup and foundational work.

3. **Required Employee Types**: List the types of employees/roles needed (e.g., Backend Developer, UI/UX Designer, QA Tester).

4. **Team Composition**: Suggest the ideal team size and composition (how many of each role, estimated project duration).

5. **Insights**: Provide 2-3 sentences of strategic advice or considerations for this project.

IMPORTANT: Respond in this exact JSON format (no additional text, no markdown):
{{
  ""suggestedFeatures"": ""• Feature 1\n• Feature 2\n• Feature 3"",
  ""recommendedTasks"": ""• Task 1\n• Task 2\n• Task 3"",
  ""requiredEmployeeTypes"": ""• Role 1\n• Role 2\n• Role 3"",
  ""teamComposition"": ""Team Size: X members\nDuration: Y months\n• Role breakdown here"",
  ""insights"": ""Your strategic insights and recommendations here.""
}}

Use bullet points (•) for lists. Keep each item concise but informative.";

            return prompt;
        }

        private async Task<string> CallGeminiApi(string prompt)
        {
            // Try both v1 and v1beta endpoints
            var urls = new[]
            {
                $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}",
                $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}"
            };

            Exception? lastException = null;

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
                        // Extract error message
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
                // Extract JSON from response (handle markdown code blocks)
                var jsonStart = geminiResponse.IndexOf('{');
                var jsonEnd = geminiResponse.LastIndexOf('}');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonText = geminiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

                    using var doc = JsonDocument.Parse(jsonText);
                    var root = doc.RootElement;

                    var features = root.GetProperty("suggestedFeatures").GetString() ?? "No features suggested";
                    var tasks = root.GetProperty("recommendedTasks").GetString() ?? "No tasks suggested";
                    var roles = root.GetProperty("requiredEmployeeTypes").GetString() ?? "No roles suggested";
                    var team = root.GetProperty("teamComposition").GetString() ?? "No team composition suggested";
                    var insights = root.GetProperty("insights").GetString() ?? "No insights provided";

                    return new ProjectSuggestionResult(
                        features,
                        tasks,
                        roles,
                        team,
                        $"?? Gemini AI Analysis: {insights}"
                    );
                }
            }
            catch
            {
                // Parsing failed, return default
            }

            return CreateDefaultSuggestion("Failed to parse Gemini response");
        }

        private ProjectSuggestionResult CreateDefaultSuggestion(string reason)
        {
            return new ProjectSuggestionResult(
                "• Define project scope\n• Set up development environment\n• Create initial documentation",
                "• Research and planning\n• Setup project infrastructure\n• Create initial design mockups",
                "• Project Manager\n• Software Developer\n• Designer",
                "Team Size: 3-5 members\nDuration: 2-3 months\n• 1 PM\n• 2 Developers\n• 1 Designer",
                reason
            );
        }
    }
}
