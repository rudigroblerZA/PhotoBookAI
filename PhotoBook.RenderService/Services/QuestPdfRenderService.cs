using System.ComponentModel;
using System.Reflection.Metadata;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhotoBook.RenderService.Data;
using PhotoBook.RenderService.Models;
using PhotoBook.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static System.Net.WebRequestMethods;

namespace PhotoBook.RenderService.Services;

public interface IPdfRenderService
{
    Task<RenderResponse> RenderPhotoBookAsync(Guid photoBookId, Guid userId, RenderQuality quality);
    Task<RenderStatus?> GetRenderStatusAsync(Guid jobId);
    Task<byte[]> RenderPagePreviewAsync(Guid photoBookId, int pageNumber);
}

public class QuestPdfRenderService : IPdfRenderService
{
    private readonly BlobServiceClient _blobClient;
    private readonly RenderDbContext _context;
    private readonly IPhotoFetchService _photoFetchService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<QuestPdfRenderService> _logger;

    public QuestPdfRenderService(
        BlobServiceClient blobClient,
        RenderDbContext context,
        IPhotoFetchService photoFetchService,
        IHttpClientFactory httpClientFactory,
        ILogger<QuestPdfRenderService> logger)
    {
        _blobClient = blobClient;
        _context = context;
        _photoFetchService = photoFetchService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<RenderResponse> RenderPhotoBookAsync(Guid photoBookId, Guid userId, RenderQuality quality)
    {
        var job = new RenderJob
        {
            Id = Guid.NewGuid(),
            PhotoBookId = photoBookId,
            UserId = userId,
            Quality = quality,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.RenderJobs.Add(job);
        await _context.SaveChangesAsync();

        return new RenderResponse(job.Id, "Pending", StartedAt: job.CreatedAt);

        //_ = https://url.za.m.mimecastprotect.com/s/n8QHCoYnNBiGJXJDSou5HplyW8?domain=task.run(async () => await ProcessRenderJobAsync(job.Id));

        //return new RenderResponse(https://url.za.m.mimecastprotect.com/s/CJjZCpgoODsLPzP9S7CjHGn-XM?domain=job.id, "Processing", StartedAt: job.CreatedAt);
    }

    private async Task ProcessRenderJobAsync(Guid jobId)
    {
        var job = await _context.RenderJobs.FindAsync(jobId);
        if (job == null) return;

        try
        {
            job.Status = "Processing";
            job.StartedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            PhotoBook.Shared.Models.PhotoBook? photoBook = await FetchPhotoBookAsync(job.PhotoBookId);
            if (photoBook == null)
                throw new InvalidOperationException("Photo book not found");

            job.TotalPages = photoBook.Pages.Count;
            await _context.SaveChangesAsync();

            var theme = await FetchThemeAsync(photoBook.Theme);
            var dpi = (int)job.Quality;
            var pdfBytes = await GeneratePdfAsync(photoBook, theme, dpi, job);

            var pdfUrl = await UploadPdfAsync(job.UserId, job.PhotoBookId, https://url.za.m.mimecastprotect.com/s/CJjZCpgoODsLPzP9S7CjHGn-XM?domain=job.id, pdfBytes);

            job.PdfBlobUrl = pdfUrl;
            job.PdfFileSize = pdfBytes.Length;
            job.Status = "Completed";
            job.Progress = 100;
            job.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Completed job {JobId}, PDF: {Size} bytes", jobId, pdfBytes.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job {JobId}", jobId);
            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task<byte[]> GeneratePdfAsync(PhotoBook photoBook, ThemeData theme, int dpi, RenderJob job)
    {
        var document = Document.Create(container =>
        {
    https://url.za.m.mimecastprotect.com/s/7SFHCqjpPEhrGOG1ivFQHEynRY?domain=container.page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontFamily(theme.FontFamily).FontSize(theme.FontSize).FontColor(theme.PrimaryColor));

            page.Header().Height(60).Background(theme.PrimaryColor).Padding(10).AlignCenter().AlignMiddle()
                .Text(photoBook.Title).FontSize(24).Bold().FontColor(theme.SecondaryColor);

            page.Content().Column(column =>
            {
                for (int i = 0; i < photoBook.Pages.Count; i++)
                {
                    if (i > 0) column.Item().PageBreak();

                    var bookPage = photoBook.Pages[i];
                    column.Item().Height(page.Size().Height - 120)
                        .Background(theme.BackgroundColor)
                        .Border(theme.BorderWidth).BorderColor(theme.BorderColor)
                        .Padding(15)
                        .Element(c => ComposePageAsync(c, bookPage, theme, job).Wait());

                    job.Progress = ((i + 1) * 100) / photoBook.Pages.Count;
                    _context.SaveChangesAsync().Wait();
                }
            });

            page.Footer().Height(30).AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            }).FontSize(10).FontColor(theme.PrimaryColor);
        });
    });

        return document.GeneratePdf();
    }

    private async Task ComposePageAsync(IContainer container, BookPage page, ThemeData theme, RenderJob job)
    {
        await Task.CompletedTask;

        container.Column(column =>
        {
            foreach (var element in page.Elements)
            {
                if (element.Type == "photo" && element.PhotoId.HasValue)
                {
                    try
                    {
                        var photoBytes = _photoFetchService.FetchPhotoAsync(element.PhotoId.Value).Result;

                        if (photoBytes != null && photoBytes.Length > 0)
                        {
                            var width = (float)(element.Position.Width * 500);
                            var height = (float)(element.Position.Height * 700);

                            column.Item()
                                .PaddingTop((float)(element.Position.Y * 100))
                                .PaddingLeft((float)(element.Position.X * 100))
                                .Width(width).Height(height).Image(photoBytes);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load photo {PhotoId}", element.PhotoId);
                        column.Item().Width(200).Height(150).Background(Colors.Grey.Lighten3)
                            .AlignCenter().AlignMiddle().Text("Photo unavailable").FontSize(10);
                    }
                }
                else if (element.Type == "text" && !string.IsNullOrEmpty(element.TextContent))
                {
                    column.Item()
                        .PaddingTop((float)(element.Position.Y * 100))
                        .PaddingLeft((float)(element.Position.X * 100))
                        .Text(element.TextContent)
                        .FontSize(theme.FontSize).FontColor(theme.PrimaryColor);
                }
            }
        });
    }

    private async Task<string> UploadPdfAsync(Guid userId, Guid photoBookId, Guid jobId, byte[] pdfBytes)
    {
        var containerClient = _blobClient.GetBlobContainerClient("renders");
        await containerClient.CreateIfNotExistsAsync();

        var blobName = $"{userId}/{photoBookId}/{jobId}.pdf";
        var blobClient = containerClient.GetBlobClient(blobName);

        using var stream = new MemoryStream(pdfBytes);
        await blobClient.UploadAsync(stream, overwrite: true);

        return blobClient.Uri.ToString();
    }

    public async Task<RenderStatus?> GetRenderStatusAsync(Guid jobId)
    {
        var job = await _context.RenderJobs.FindAsync(jobId);
        if (job == null) return null;

        return new RenderStatus(https://url.za.m.mimecastprotect.com/s/CJjZCpgoODsLPzP9S7CjHGn-XM?domain=job.id, job.Status, job.Progress, job.TotalPages, 
            job.PdfBlobUrl, job.ErrorMessage, job.CreatedAt, job.CompletedAt);
    }

    public async Task<byte[]> RenderPagePreviewAsync(Guid photoBookId, int pageNumber)
    {
        var photoBook = await FetchPhotoBookAsync(photoBookId);
        if (photoBook == null) throw new InvalidOperationException("Photo book not found");

        var page = photoBook.Pages.FirstOrDefault(p => p.PageNumber == pageNumber);
        if (page == null) throw new InvalidOperationException("Page not found");

        var theme = await FetchThemeAsync(photoBook.Theme);

        var document = Document.Create(container =>
        {
    https://url.za.m.mimecastprotect.com/s/7SFHCqjpPEhrGOG1ivFQHEynRY?domain=container.page(p =>
        {
            p.Size(PageSizes.A4);
            p.Margin(20);
            p.Background(theme.BackgroundColor);
            p.Content().Element(c => ComposePageAsync(c, page, theme, new RenderJob()).Wait());
        });
    });

        return document.GeneratePdf();
    }

    private async Task<PhotoBook?> FetchPhotoBookAsync(Guid photoBookId)
{
    try
    {
        var client = _httpClientFactory.CreateClient("DesignService");
        var response = await client.GetAsync($"/api/design/photobook/{photoBookId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PhotoBook>();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching photobook {PhotoBookId}", photoBookId);
        return null;
    }
}

private async Task<ThemeData> FetchThemeAsync(string themeName)
{
    await Task.CompletedTask;

    return themeName.ToLower() switch
    {
        "modern" => new ThemeData { PrimaryColor = "#000000", SecondaryColor = "#FFFFFF", AccentColor = "#FFD700", FontFamily = "Helvetica", FontSize = 16, BackgroundColor = "#FFFFFF" },
        "vintage" => new ThemeData { PrimaryColor = "#8B4513", SecondaryColor = "#F5DEB3", AccentColor = "#CD853F", FontFamily = "Times New Roman", FontSize = 13, BackgroundColor = "#FFF8DC", BorderStyle = "ornate", BorderWidth = 3, BorderColor = "#8B4513" },
        _ => new ThemeData { PrimaryColor = "#2C3E50", SecondaryColor = "#ECF0F1", AccentColor = "#E74C3C", FontFamily = "Georgia", FontSize = 14, BackgroundColor = "#FFFFFF", BorderStyle = "simple", BorderWidth = 2, BorderColor = "#2C3E50" }
    };
}
}
