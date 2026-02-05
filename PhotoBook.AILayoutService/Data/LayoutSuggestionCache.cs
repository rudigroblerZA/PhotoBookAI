namespace PhotoBook.AILayoutService.Data;

public class LayoutSuggestionCache
{
    public Guid Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string SuggestionsJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
