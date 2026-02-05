using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PhotoBook.AILayoutService.Data;
using PhotoBook.AILayoutService.Models;
using PhotoBook.Shared.Models;

namespace PhotoBook.AILayoutService.Services;

public class PhotoAnalysisService : IPhotoAnalysisService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LayoutDbContext _context;
    private readonly ILogger<PhotoAnalysisService> _logger;

    public PhotoAnalysisService(
        IHttpClientFactory httpClientFactory,
        LayoutDbContext context,
        ILogger<PhotoAnalysisService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _logger = logger;
    }

    public async Task<List<PhotoAnalysisResult>> AnalyzePhotosAsync(List<Guid> photoIds)
    {
        var results = new List<PhotoAnalysisResult>();

        foreach (var photoId in photoIds)
        {
            // Check cache first
            var cached = await _context.PhotoAnalysisCache
                .FirstOrDefaultAsync(c => c.PhotoId == photoId);

            if (cached != null &&
                cached.CreatedAt > DateTime.UtcNow.AddDays(-7))
            {
                var cachedResult = JsonSerializer.Deserialize<PhotoAnalysisResult>(cached.AnalysisJson);
                if (cachedResult != null)
                {
                    results.Add(cachedResult);
                    continue;
                }
            }

            // Fetch fresh analysis
            var analysis = await AnalyzePhotoAsync(photoId);
            results.Add(analysis);

            // Cache result
            var cacheEntry = new PhotoAnalysisCache
            {
                PhotoId = photoId,
                AnalysisJson = JsonSerializer.Serialize(analysis),
                CreatedAt = DateTime.UtcNow
            };

            _context.PhotoAnalysisCache.Add(cacheEntry);
        }

        await _context.SaveChangesAsync();
        return results;
    }

    private async Task<PhotoAnalysisResult> AnalyzePhotoAsync(Guid photoId)
    {
        try
        {
            // Fetch photo metadata from PhotoService
            var client = _httpClientFactory.CreateClient("PhotoService");
            var response = await client.GetAsync($"/api/photo/{photoId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch photo {PhotoId}", photoId);
                return CreateDefaultAnalysis(photoId);
            }

            var photoMeta = await response.Content.ReadFromJsonAsync<PhotoMetadata>();
            if (photoMeta == null)
            {
                return CreateDefaultAnalysis(photoId);
            }

            // Determine orientation
            string orientation;
            double aspectRatio = (double)photoMeta.Width / photoMeta.Height;

            if (aspectRatio > 1.2)
                orientation = "landscape";
            else if (aspectRatio < 0.8)
                orientation = "portrait";
            else
                orientation = "square";

            // Calculate quality score
            var quality = CalculateQuality(photoMeta.Width, photoMeta.Height, photoMeta.FileSize);

            // Estimate brightness (would need actual image analysis in production)
            var brightness = 0.5; // Default mid-brightness

            return new PhotoAnalysisResult
            {
                PhotoId = photoId,
                Orientation = orientation,
                Width = photoMeta.Width,
                Height = photoMeta.Height,
                AspectRatio = aspectRatio,
                //DateTaken = photoMeta.DateTaken,
                Location = photoMeta.Location,
                Brightness = brightness,
                Contrast = 0.5,
                HasFaces = false, // Would require face detection
                FaceCount = 0,
                Quality = quality,
                DominantColors = new List<string> { "#CCCCCC" } // Would require color analysis
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing photo {PhotoId}", photoId);
            return CreateDefaultAnalysis(photoId);
        }
    }

    private PhotoAnalysisResult CreateDefaultAnalysis(Guid photoId)
    {
        return new PhotoAnalysisResult
        {
            PhotoId = photoId,
            Orientation = "landscape",
            Width = 1920,
            Height = 1080,
            AspectRatio = 1.78,
            Quality = "medium"
        };
    }

    private string CalculateQuality(int width, int height, long fileSize)
    {
        var megapixels = (width * height) / 1_000_000.0;

        if (megapixels >= 12 && fileSize > 2_000_000)
            return "excellent";

        if (megapixels >= 8 && fileSize > 1_000_000)
            return "good";

        if (megapixels >= 4)
            return "medium";

        return "low";
    }
}
