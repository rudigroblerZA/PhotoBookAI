using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PhotoBook.AILayoutService.Data;
using PhotoBook.Shared.Contracts;

namespace PhotoBook.AILayoutService.Services;

public class LayoutCacheService : ILayoutCacheService
{
    private readonly LayoutDbContext _context;
    private readonly ILogger<LayoutCacheService> _logger;

    public LayoutCacheService(LayoutDbContext context, ILogger<LayoutCacheService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public string GenerateCacheKey(LayoutSuggestionRequest request)
    {
        // Sort photo IDs for consistent cache keys
        var sortedIds = request.PhotoIds.OrderBy(id => id).ToList();

        var keyData = $"{string.Join(",", sortedIds)}|{request.DesiredPageCount}|{request.PreferredTheme ?? "none"}";

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyData));
        return Convert.ToBase64String(hashBytes);
    }

    public async Task<List<LayoutSuggestion>?> GetCachedSuggestionsAsync(string cacheKey)
    {
        try
        {
            var cached = await _context.LayoutCache
                .FirstOrDefaultAsync(c => c.CacheKey == cacheKey && c.ExpiresAt > DateTime.UtcNow);

            if (cached == null)
                return null;

            var suggestions = JsonSerializer.Deserialize<List<LayoutSuggestion>>(cached.SuggestionsJson);
            _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached suggestions");
            return null;
        }
    }

    public async Task CacheSuggestionsAsync(string cacheKey, List<LayoutSuggestion> suggestions)
    {
        try
        {
            var cacheEntry = new LayoutSuggestionCache
            {
                Id = Guid.NewGuid(),
                CacheKey = cacheKey,
                SuggestionsJson = JsonSerializer.Serialize(suggestions),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _context.LayoutCache.Add(cacheEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cached suggestions with key {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching suggestions");
        }
    }
}
