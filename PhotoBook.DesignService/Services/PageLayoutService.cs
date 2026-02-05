using PhotoBook.Shared.Models;

namespace PhotoBook.DesignService.Services;

public interface IPageLayoutService
{
    Task<List<BookPage>> GenerateAutomaticLayoutAsync(
        List<Guid> photoIds,
        string layoutStyle,
        int startPage = 1);
}

public class PageLayoutService : IPageLayoutService
{
    private readonly ILogger<PageLayoutService> _logger;

    public PageLayoutService(ILogger<PageLayoutService> logger)
    {
        _logger = logger;
    }

    public async Task<List<BookPage>> GenerateAutomaticLayoutAsync(
        List<Guid> photoIds,
        string layoutStyle,
        int startPage = 1)
    {
        await Task.CompletedTask;

        var pages = new List<BookPage>();
        int currentPage = startPage;
        int photoIndex = 0;

        switch (layoutStyle.ToLower())
        {
            case "single":
                // One photo per page
                while (photoIndex < photoIds.Count)
                {
                    pages.Add(CreateSinglePhotoPage(currentPage, photoIds[photoIndex]));
                    photoIndex++;
                    currentPage++;
                }
                break;

            case "sidebyside":
                // Two photos per page
                while (photoIndex < photoIds.Count)
                {
                    var photos = photoIds.Skip(photoIndex).Take(2).ToList();
                    pages.Add(CreateSideBySidePage(currentPage, photos));
                    photoIndex += 2;
                    currentPage++;
                }
                break;

            case "grid":
                // Four photos per page
                while (photoIndex < photoIds.Count)
                {
                    var photos = photoIds.Skip(photoIndex).Take(4).ToList();
                    pages.Add(CreateGridPage(currentPage, photos));
                    photoIndex += 4;
                    currentPage++;
                }
                break;

            default:
                _logger.LogWarning("Unknown layout style: {Style}, using single", layoutStyle);
                return await GenerateAutomaticLayoutAsync(photoIds, "single", startPage);
        }

        return pages;
    }

    private BookPage CreateSinglePhotoPage(int pageNumber, Guid photoId)
    {
        return new BookPage
        {
            PageNumber = pageNumber,
            Elements = new List<PageElement>
            {
                new PageElement
                {
                    Type = "photo",
                    PhotoId = photoId,
                    Position = new PositionData
                    {
                        X = 0.1,
                        Y = 0.1,
                        Width = 0.8,
                        Height = 0.8
                    }
                }
            }
        };
    }

    private BookPage CreateSideBySidePage(int pageNumber, List<Guid> photoIds)
    {
        var elements = new List<PageElement>();

        for (int i = 0; i < photoIds.Count && i < 2; i++)
        {
            elements.Add(new PageElement
            {
                Type = "photo",
                PhotoId = photoIds[i],
                Position = new PositionData
                {
                    X = i == 0 ? 0.05 : 0.55,
                    Y = 0.1,
                    Width = 0.4,
                    Height = 0.8
                }
            });
        }

        return new BookPage
        {
            PageNumber = pageNumber,
            Elements = elements
        };
    }

    private BookPage CreateGridPage(int pageNumber, List<Guid> photoIds)
    {
        var elements = new List<PageElement>();
        var positions = new[]
        {
            (0.05, 0.05), (0.55, 0.05),
            (0.05, 0.55), (0.55, 0.55)
        };

        for (int i = 0; i < photoIds.Count && i < 4; i++)
        {
            elements.Add(new PageElement
            {
                Type = "photo",
                PhotoId = photoIds[i],
                Position = new PositionData
                {
                    X = positions[i].Item1,
                    Y = positions[i].Item2,
                    Width = 0.4,
                    Height = 0.4
                }
            });
        }

        return new BookPage
        {
            PageNumber = pageNumber,
            Elements = elements
        };
    }
}
