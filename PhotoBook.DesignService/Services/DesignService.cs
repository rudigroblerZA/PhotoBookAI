using Microsoft.EntityFrameworkCore;
using PhotoBook.DesignService.Data;
using PhotoBook.Shared.Models;

namespace PhotoBook.DesignService.Services;

public interface IDesignService
{
    Task<PhotoBook.Shared.Models.PhotoBook> CreatePhotoBookAsync(Guid userId, string title, Guid? themeId);
    Task<PhotoBook.Shared.Models.PhotoBook?> GetPhotoBookAsync(Guid id);
    Task<List<PhotoBook.Shared.Models.PhotoBook>> GetUserPhotoBooksAsync(Guid userId, int skip, int take);
    Task UpdatePhotoBookAsync(Guid id, string? title, BookStatus? status);
    Task<bool> DeletePhotoBookAsync(Guid id, Guid userId);
    Task ApplyThemeAsync(Guid photoBookId, Guid themeId);
    Task UpdatePageAsync(Guid photoBookId, int pageNumber, List<PageElement> elements);
    Task AddTextElementAsync(Guid photoBookId, int pageNumber, string text, PositionData position, string? fontFamily, int? fontSize, string? color);
    Task AddPhotoElementAsync(Guid photoBookId, int pageNumber, Guid photoId, PositionData position, string? filterId);
    Task RemovePageElementAsync(Guid photoBookId, int pageNumber, int elementIndex);
    Task ApplyTemplateAsync(Guid photoBookId, Guid templateId, List<Guid> photoIds, int startPage);
    Task<string> GeneratePreviewAsync(Guid photoBookId, int pageNumber);
}

public class DesignService : IDesignService
{
    private readonly DesignDbContext _context;
    private readonly IThemeService _themeService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<DesignService> _logger;

    public DesignService(
        DesignDbContext context,
        IThemeService themeService,
        ITemplateService templateService,
        ILogger<DesignService> logger)
    {
        _context = context;
        _themeService = themeService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<PhotoBook.Shared.Models.PhotoBook> CreatePhotoBookAsync(Guid userId, string title, Guid? themeId)
    {
        var themeName = "Classic";
        if (themeId.HasValue)
        {
            var theme = await _context.Themes.FindAsync(themeId.Value);
            if (theme != null)
                themeName = theme.Name;
        }

        var photoBook = new PhotoBook.Shared.Models.PhotoBook
        {
            Id = Guid.Parse("12121212-1111-1111-1111-111111111111"),
            //Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Theme = themeName,
            Status = BookStatus.Draft,
            //CreatedAt = DateTime.UtcNow,
            Pages = new List<BookPage>()
        };

        _context.PhotoBooks.Add(photoBook);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Photo book created: {PhotoBookId} for user {UserId}",
            photoBook.Id, userId);

        return photoBook;
    }

    public async Task<PhotoBook.Shared.Models.PhotoBook?> GetPhotoBookAsync(Guid id)
    {
        return await _context.PhotoBooks.FindAsync(id);
    }

    public async Task<List<PhotoBook.Shared.Models.PhotoBook>> GetUserPhotoBooksAsync(Guid userId, int skip, int take)
    {
        return await _context.PhotoBooks
            .Where(pb => pb.UserId == userId)
            //.OrderByDescending(pb => pb.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task UpdatePhotoBookAsync(Guid id, string? title, BookStatus? status)
    {
        var photoBook = await _context.PhotoBooks.FindAsync(id);
        if (photoBook == null)
            throw new InvalidOperationException("Photo book not found");

        if (!string.IsNullOrEmpty(title))
            photoBook.Title = title;

        if (status.HasValue)
            photoBook.Status = status.Value;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeletePhotoBookAsync(Guid id, Guid userId)
    {
        var photoBook = await _context.PhotoBooks
            .FirstOrDefaultAsync(pb => pb.Id == id && pb.UserId == userId);

        if (photoBook == null)
            return false;

        _context.PhotoBooks.Remove(photoBook);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Photo book deleted: {PhotoBookId}", id);
        return true;
    }

    public async Task ApplyThemeAsync(Guid photoBookId, Guid themeId)
    {
        var photoBook = await _context.PhotoBooks.FindAsync(photoBookId);
        if (photoBook == null)
            throw new InvalidOperationException("Photo book not found");

        var theme = await _context.Themes.FindAsync(themeId);
        if (theme == null)
            throw new InvalidOperationException("Theme not found");

        photoBook.Theme = theme.Name;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Theme {ThemeName} applied to photo book {PhotoBookId}",
            theme.Name, photoBookId);
    }

    public async Task UpdatePageAsync(Guid photoBookId, int pageNumber, List<PageElement> elements)
    {
        var photoBook = await _context.PhotoBooks.FindAsync(photoBookId);
        if (photoBook == null)
            throw new InvalidOperationException("Photo book not found");

        var page = photoBook.Pages.FirstOrDefault(p => p.PageNumber == pageNumber);
        if (page == null)
        {
            page = new BookPage { PageNumber = pageNumber };
            photoBook.Pages.Add(page);
            photoBook.Pages = photoBook.Pages.OrderBy(p => p.PageNumber).ToList();
        }

        page.Elements = elements;
        photoBook.PageCount = photoBook.Pages.Count;

        _context.Entry(photoBook).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task AddTextElementAsync(
        Guid photoBookId,
        int pageNumber,
        string text,
        PositionData position,
        string? fontFamily,
        int? fontSize,
        string? color)
    {
        var photoBook = await _context.PhotoBooks.FindAsync(photoBookId);
        if (photoBook == null)
            throw new InvalidOperationException("Photo book not found");

        var page = photoBook.Pages.FirstOrDefault(p => p.PageNumber == pageNumber);
        if (page == null)
        {
            page = new BookPage { PageNumber = pageNumber, Elements = new() };
            photoBook.Pages.Add(page);
        }

        page.Elements.Add(new PageElement
        {
            Type = "text",
            TextContent = text,
            Position = position
        });

        photoBook.PageCount = Math.Max(photoBook.PageCount, photoBook.Pages.Count);
        _context.Entry(photoBook).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task AddPhotoElementAsync(
        Guid photoBookId,
        int pageNumber,
        Guid photoId,
        PositionData position,
        string? filterId)
    {
        var photoBook = await _context.PhotoBooks.FindAsync(photoBookId);
        if (photoBook == null)
            throw new InvalidOperationException("Photo book not found");

        var page = photoBook.Pages.FirstOrDefault(p => p.PageNumber == pageNumber);
        if (page == null)
        {
            page = new BookPage { PageNumber = pageNumber, Elements = new() };
            photoBook.Pages.Add(page);
        }

        page.Elements.Add(new PageElement
        {
            Type = "photo",
            PhotoId = photoId,
            Position = position
        });

        photoBook.PageCount = Math.Max(photoBook.PageCount, photoBook.Pages.Count);
        _context.Entry(photoBook).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task RemovePageElementAsync(Guid photoBookId, int pageNumber, int elementIndex)
    {
        var photoBook = await _context.PhotoBooks.FindAsync(photoBookId);
        if (photoBook == null)
            throw new InvalidOperationException("Photo book not found");

        var page = photoBook.Pages.FirstOrDefault(p => p.PageNumber == pageNumber);
        if (page == null || elementIndex < 0 || elementIndex >= page.Elements.Count)
            throw new InvalidOperationException("Invalid element index");

        page.Elements.RemoveAt(elementIndex);

        _context.Entry(photoBook).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task ApplyTemplateAsync(
        Guid photoBookId,
        Guid templateId,
        List<Guid> photoIds,
        int startPage)
    {
        var photoBook = await _context.PhotoBooks.FindAsync(photoBookId);
        if (photoBook == null)
            throw new InvalidOperationException("Photo book not found");

        var template = await _templateService.GetTemplateAsync(templateId);
        if (template == null)
            throw new InvalidOperationException("Template not found");

        var layout = await _templateService.ParseTemplateLayoutAsync(template);

        int currentPage = startPage;
        int photoIndex = 0;

        while (photoIndex < photoIds.Count)
        {
            var page = photoBook.Pages.FirstOrDefault(p => p.PageNumber == currentPage);
            if (page == null)
            {
                page = new BookPage
                {
                    PageNumber = currentPage,
                    LayoutTemplateId = template.Id.ToString(),
                    Elements = new()
                };
                photoBook.Pages.Add(page);
            }
            else
            {
                page.Elements.Clear();
                page.LayoutTemplateId = template.Id.ToString();
            }

            // Add photos according to template slots
            for (int i = 0; i < layout.Slots.Count && photoIndex < photoIds.Count; i++)
            {
                var slot = layout.Slots[i];
                page.Elements.Add(new PageElement
                {
                    Type = "photo",
                    PhotoId = photoIds[photoIndex],
                    Position = new PositionData
                    {
                        X = slot.X,
                        Y = slot.Y,
                        Width = slot.Width,
                        Height = slot.Height
                    }
                });
                photoIndex++;
            }

            currentPage++;
        }

        photoBook.Pages = photoBook.Pages.OrderBy(p => p.PageNumber).ToList();
        photoBook.PageCount = photoBook.Pages.Count;

        _context.Entry(photoBook).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Template {TemplateName} applied to photo book {PhotoBookId}",
            template.Name, photoBookId);
    }

    public async Task<string> GeneratePreviewAsync(Guid photoBookId, int pageNumber)
    {
        // TODO: Implement actual preview generation
        // This would render the page to an image and upload to blob storage
        await Task.CompletedTask;

        var snapshot = new PageSnapshot
        {
            Id = Guid.NewGuid(),
            PhotoBookId = photoBookId,
            PageNumber = pageNumber,
            SnapshotUrl = $"https://preview.placeholder.com/{photoBookId}/{pageNumber}",
            CreatedAt = DateTime.UtcNow
        };

        _context.PageSnapshots.Add(snapshot);
        await _context.SaveChangesAsync();

        return snapshot.SnapshotUrl;
    }
}
