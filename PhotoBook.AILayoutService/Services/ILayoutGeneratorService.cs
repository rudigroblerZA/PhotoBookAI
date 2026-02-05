using PhotoBook.AILayoutService.Models;
using PhotoBook.Shared.Contracts;

namespace PhotoBook.AILayoutService.Services;

public interface ILayoutGeneratorService
{
    Task<List<LayoutSuggestion>> GenerateLayoutSuggestionsAsync(LayoutSuggestionRequest request);
    Task<LayoutSuggestion> GenerateSmartLayoutAsync(List<Guid> photoIds, Guid userId, string style, int desiredPageCount);
    Task<LayoutRecommendations> GetLayoutRecommendationsAsync(List<Guid> photoIds);
}
