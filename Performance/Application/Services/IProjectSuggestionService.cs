using System.Threading.Tasks;

namespace Performance.Application.Services
{
    public class ProjectSuggestionResult
    {
        public string SuggestedFeatures { get; }
        public string RecommendedTasks { get; }
        public string RequiredEmployeeTypes { get; }
        public string TeamComposition { get; }
        public string Explanation { get; }

        public ProjectSuggestionResult(
            string suggestedFeatures, 
            string recommendedTasks, 
            string requiredEmployeeTypes, 
            string teamComposition,
            string explanation)
        {
            SuggestedFeatures = suggestedFeatures;
            RecommendedTasks = recommendedTasks;
            RequiredEmployeeTypes = requiredEmployeeTypes;
            TeamComposition = teamComposition;
            Explanation = explanation;
        }
    }

    public interface IProjectSuggestionService
    {
        Task<ProjectSuggestionResult> SuggestProjectDetailsAsync(string projectName, string projectDescription);
    }
}
