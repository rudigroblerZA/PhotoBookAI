using PhotoBook.Shared.Contracts;

namespace PhotoBook.AILayoutService.Services;

public interface ILayoutCacheService
{
    string GenerateCacheKey(LayoutSuggestionRequest request);
    Task<List<LayoutSuggestion>?> GetCachedSuggestionsAsync(string cacheKey);
    Task CacheSuggestionsAsync(string cacheKey, List<LayoutSuggestion> suggestions);
}
