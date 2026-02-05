using System.Globalization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using PhotoBook.PhotoService.Data;
using PhotoBook.PhotoService.Models;
using PhotoBook.Shared.Models;

namespace PhotoBook.PhotoService.Services;

public class PhotoUploadService : IPhotoUploadService
{
    private readonly PhotoDbContext _context;
    private readonly BlobServiceClient _blobClient;
    private readonly IImageProcessingService _imageProcessing;
    private readonly IMetadataExtractionService _metadataExtraction;
    private readonly ILogger<PhotoUploadService> _logger;

    public PhotoUploadService(
        PhotoDbContext context,
        BlobServiceClient blobClient,
        IImageProcessingService imageProcessing,
        IMetadataExtractionService metadataExtraction,
        ILogger<PhotoUploadService> logger)
    {
        _context = context;
        _blobClient = blobClient;
        _imageProcessing = imageProcessing;
        _metadataExtraction = metadataExtraction;
        _logger = logger;
    }

    public async Task<PhotoMetadata> UploadPhotoAsync(IFormFile file, Guid userId)
    {
        var photoId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // Get blob container
        var containerClient = _blobClient.GetBlobContainerClient("photos");
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        // Upload original photo
        var blobName = $"{userId}/{photoId}{extension}";
        var blobClient = containerClient.GetBlobClient(blobName);

        using var stream = file.OpenReadStream();

        // Set content type
        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = file.ContentType
        };

        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        });

        _logger.LogInformation("Photo uploaded to blob storage: {BlobName}", blobName);

        // Extract metadata
        stream.Position = 0;
        var (width, height, dateTaken, location, orientation) =
            await _metadataExtraction.ExtractMetadataAsync(stream);

        // Generate thumbnail
        stream.Position = 0;
        var thumbnailUrl = await _imageProcessing.GenerateThumbnailAsync(
            stream, userId, photoId, extension);

        // Create metadata record
        var photo = new PhotoMetadata
        {
            Id = photoId,
            UserId = userId,
            FileName = file.FileName,
            BlobUrl = blobClient.Uri.ToString(),
            ThumbnailUrl = thumbnailUrl,
            FileSize = file.Length,
            Width = width,
            Height = height,
            DateTaken = dateTaken,
            Location = location,
            UploadedAt = DateTime.UtcNow,
        };

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Photo metadata saved: {PhotoId}", photoId);

        return photo;
    }

    public async Task<List<PhotoMetadata>> GetUserPhotosAsync(
        Guid userId,
        int skip,
        int take,
        string? sortBy,
        bool descending)
    {
        var query = _context.Photos.Where(p => p.UserId == userId);

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "datetaken" => descending
                ? query.OrderByDescending(p => p.DateTaken ?? p.UploadedAt)
                : query.OrderBy(p => p.DateTaken ?? p.UploadedAt),
            "filename" => descending
                ? query.OrderByDescending(p => p.FileName)
                : query.OrderBy(p => p.FileName),
            "size" => descending
                ? query.OrderByDescending(p => p.FileSize)
                : query.OrderBy(p => p.FileSize),
            _ => descending
                ? query.OrderByDescending(p => p.FileName)
                : query.OrderBy(p => p.FileName)
        };

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<PhotoMetadata?> GetPhotoAsync(Guid photoId)
    {
        return await _context.Photos.FindAsync(photoId);
    }

    public async Task<bool> DeletePhotoAsync(Guid photoId, Guid userId)
    {
        var photo = await _context.Photos
            .FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == userId);

        if (photo == null)
            return false;

        try
        {
            // Delete from blob storage
            var containerClient = _blobClient.GetBlobContainerClient("photos");
            var extension = Path.GetExtension(photo.FileName);

            // Delete original
            var blobName = $"{userId}/{photoId}{extension}";
            await containerClient.DeleteBlobIfExistsAsync(blobName);

            // Delete thumbnail
            var thumbnailName = $"{userId}/thumbnails/{photoId}{extension}";
            await containerClient.DeleteBlobIfExistsAsync(thumbnailName);

            _logger.LogInformation("Blobs deleted for photo {PhotoId}", photoId);

            // Delete from database
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo {PhotoId}", photoId);
            throw;
        }
    }

    public async Task<object> GetUserStatsAsync(Guid userId)
    {
        var photos = await _context.Photos
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return new
        {
            totalPhotos = photos.Count,
            totalSize = photos.Sum(p => p.FileSize),
            oldestPhoto = photos.Min(p => p.DateTaken ?? p.UploadedAt),
            newestPhoto = photos.Max(p => p.DateTaken ?? p.UploadedAt),
            averageWidth = photos.Average(p => p.Width),
            averageHeight = photos.Average(p => p.Height)
        };
    }
}
