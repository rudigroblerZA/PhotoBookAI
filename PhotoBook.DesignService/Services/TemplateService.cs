using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PhotoBook.DesignService.Data;

namespace PhotoBook.DesignService.Services;

public interface ITemplateService
{
    Task<List<DesignTemplate>> GetTemplatesAsync(string? category = null);
    Task<DesignTemplate?> GetTemplateAsync(Guid id);
    Task<TemplateLayout> ParseTemplateLayoutAsync(DesignTemplate template);
}

public class TemplateService : ITemplateService
{
    private readonly DesignDbContext _context;
    private readonly ILogger<TemplateService> _logger;

    public TemplateService(DesignDbContext context, ILogger<TemplateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<DesignTemplate>> GetTemplatesAsync(string? category = null)
    {
        var query = _context.Templates.Where(t => t.IsActive);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(t => t.Category == category);
        }

        return await query
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<DesignTemplate?> GetTemplateAsync(Guid id)
    {
        return await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
    }

    public async Task<TemplateLayout> ParseTemplateLayoutAsync(DesignTemplate template)
    {
        await Task.CompletedTask;

        try
        {
            var layout = JsonSerializer.Deserialize<TemplateLayout>(template.LayoutJson);
            return layout ?? new TemplateLayout { Slots = new() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing template layout for {TemplateName}", template.Name);
            return new TemplateLayout { Slots = new() };
        }
    }
}

public class TemplateLayout
{
    public List<PhotoSlot> Slots { get; set; } = new();
}

public class PhotoSlot
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}
