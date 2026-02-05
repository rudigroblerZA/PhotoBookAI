using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhotoBook.DesignService.Data;

namespace PhotoBook.DesignService.Services;

public interface IThemeService
{
    Task<List<Theme>> GetAllThemesAsync();
    Task<Theme?> GetThemeAsync(Guid id);
    Task<Theme?> GetThemeByNameAsync(string name);
}

public class ThemeService : IThemeService
{
    private readonly DesignDbContext _context;
    private readonly IMemoryCache _cache;
    private const string THEMES_CACHE_KEY = "all_themes";
    private readonly ILogger<ThemeService> _logger;

    public ThemeService(
        DesignDbContext context,
        IMemoryCache cache,
        ILogger<ThemeService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<Theme>> GetAllThemesAsync()
    {
        if (_cache.TryGetValue(THEMES_CACHE_KEY, out List<Theme>? themes))
        {
            return themes!;
        }

        themes = await _context.Themes
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();

        _cache.Set(THEMES_CACHE_KEY, themes, TimeSpan.FromHours(24));

        _logger.LogInformation("Loaded {Count} themes", themes.Count);
        return themes;
    }

    public async Task<Theme?> GetThemeAsync(Guid id)
    {
        return await _context.Themes
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
    }

    public async Task<Theme?> GetThemeByNameAsync(string name)
    {
        return await _context.Themes
            .FirstOrDefaultAsync(t => t.Name == name && t.IsActive);
    }
}
