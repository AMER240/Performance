using System.Threading.Tasks;
using Performance.Domain.Entities;
using Performance.Domain.Enums;

namespace Performance.Application.Interfaces
{
    public record SuggestionResult(string[] SuggestedUserIds, TaskPriority SuggestedPriority, System.TimeSpan EstimatedDuration, string Explanation);

    public interface ITaskSuggestionService
    {
        Task<SuggestionResult> SuggestAsync(string taskDescription, int? projectId = null);
    }
}
