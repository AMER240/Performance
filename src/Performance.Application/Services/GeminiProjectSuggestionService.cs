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
            var prompt = $@"You are an expert project management consultant with deep experience in software development, 
team composition, and project planning. Analyze the following project thoroughly and provide comprehensive, 
detailed suggestions that demonstrate deep expertise.

PROJECT NAME: {projectName}

PROJECT DESCRIPTION: {projectDescription}

Provide detailed and comprehensive suggestions for this project. Be specific, practical, and thorough. 
Each section should have substantial content with clear explanations.

1. **SUGGESTED FEATURES** (provide 5-7 key features):
   - List each feature with a brief explanation of its value
   - Use bullet points (•)
   - Be specific to this project type and context
   - Include both core features and value-add features
   
2. **RECOMMENDED TASKS** (provide 6-8 initial tasks):
   - Prioritize setup and foundational work
   - Include both technical and planning tasks
   - Specify deliverables for each task
   - Use bullet points (•)
   - Order by priority/dependency
   
3. **REQUIRED EMPLOYEE TYPES** (list 5-7 roles):
   - Specify exact job titles and specializations
   - Include quantity needed (e.g., Backend Developer x2, Senior Frontend Developer x1)
   - Mention key skills required for each role
   - Use bullet points (•)
   
4. **TEAM COMPOSITION** (provide detailed breakdown):
   - Total team size recommendation with reasoning
   - Duration estimate (in months) with justification
   - Detailed role breakdown with specific responsibilities
   - Consider project phases (planning, development, testing, deployment)
   - Use bullet points (•) for structure
   
5. **AI INSIGHTS** (provide 3-5 paragraphs of strategic advice):
   - Technology stack recommendations with specific tools/frameworks
   - Potential challenges specific to this project type and how to mitigate them
   - Best practices and industry standards relevant to this project
   - Success factors and key milestones to track
   - Risk management and quality assurance strategies

Be detailed, comprehensive, and professional. Provide actionable insights that would be valuable 
for an experienced project manager. Each section should demonstrate deep understanding of the project context.

IMPORTANT: Respond ONLY in this exact JSON format (no markdown, no additional text):
{{
  ""suggestedFeatures"": ""• Feature 1: Detailed explanation\n• Feature 2: Detailed explanation\n• Feature 3: Detailed explanation\n• Feature 4: Detailed explanation\n• Feature 5: Detailed explanation"",
  ""recommendedTasks"": ""• Task 1: With deliverable\n• Task 2: With deliverable\n• Task 3: With deliverable\n• Task 4: With deliverable\n• Task 5: With deliverable\n• Task 6: With deliverable"",
  ""requiredEmployeeTypes"": ""• Role 1 (quantity): Key skills\n• Role 2 (quantity): Key skills\n• Role 3 (quantity): Key skills\n• Role 4 (quantity): Key skills\n• Role 5 (quantity): Key skills"",
  ""teamComposition"": ""Team Size: X members (reasoning)\nDuration: Y months (justification)\n\nRole Breakdown:\n• Role 1: Specific responsibilities\n• Role 2: Specific responsibilities\n• Role 3: Specific responsibilities"",
  ""insights"": ""Paragraph 1: Technology recommendations with specific tools.\n\nParagraph 2: Challenges and mitigation strategies.\n\nParagraph 3: Best practices and success factors.\n\nParagraph 4: Risk management approach.""
}}

Use bullet points (•) for all lists. Provide substantial, detailed content in each section.";

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
