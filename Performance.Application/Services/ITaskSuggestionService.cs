using System.Threading.Tasks;
using Performance.Infrastructure.Entities;

namespace Performance.Application.Services
{
    public record SuggestionResult(string[] SuggestedUserIds, TaskPriority SuggestedPriority, System.TimeSpan EstimatedDuration, string Explanation);

    public interface ITaskSuggestionService
    {
        Task<SuggestionResult> SuggestAsync(string taskDescription, int? projectId = null);
    }
}
