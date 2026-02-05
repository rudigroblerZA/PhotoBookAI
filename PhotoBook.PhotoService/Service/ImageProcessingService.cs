using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace PhotoBook.PhotoService.Services;

public class ImageProcessingService : IImageProcessingService
{
    private readonly BlobServiceClient _blobClient;
    private readonly ILogger<ImageProcessingService> _logger;
    private const int ThumbnailSize = 300;

    public ImageProcessingService(
        BlobServiceClient blobClient,
        ILogger<ImageProcessingService> logger)
    {
        _blobClient = blobClient;
        _logger = logger;
    }

    public async Task<string> GenerateThumbnailAsync(
        Stream imageStream,
        Guid userId,
        Guid photoId,
        string extension)
    {
        try
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(imageStream);

            // Calculate thumbnail dimensions maintaining aspect ratio
            var ratio = (double)ThumbnailSize / Math.Max(image.Width, image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(newWidth, newHeight));

            // Save to memory stream
            using var thumbnailStream = new MemoryStream();
            await image.SaveAsJpegAsync(thumbnailStream, new JpegEncoder { Quality = 85 });
            thumbnailStream.Position = 0;

            // Upload thumbnail to blob storage
            var containerClient = _blobClient.GetBlobContainerClient("photos");
            var thumbnailBlobName = $"{userId}/thumbnails/{photoId}{extension}";
            var thumbnailBlobClient = containerClient.GetBlobClient(thumbnailBlobName);

            await thumbnailBlobClient.UploadAsync(thumbnailStream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" }
            });

            _logger.LogInformation("Thumbnail generated: {ThumbnailBlobName}", thumbnailBlobName);

            return thumbnailBlobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for photo {PhotoId}", photoId);
            throw;
        }
    }

    public async Task<byte[]> ApplyFilterAsync(byte[] imageData, string filterId)
    {
        using var image = Image.Load(imageData);

        switch (filterId.ToLower())
        {
            case "grayscale":
                image.Mutate(x => x.Grayscale());
                break;
            case "sepia":
                image.Mutate(x => x.Sepia());
                break;
            case "brightness":
                image.Mutate(x => x.Brightness(1.2f));
                break;
            case "contrast":
                image.Mutate(x => x.Contrast(1.3f));
                break;
            default:
                _logger.LogWarning("Unknown filter: {FilterId}", filterId);
                break;
        }

        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 90 });
        return outputStream.ToArray();
    }

    public async Task<byte[]> ResizeImageAsync(byte[] imageData, int maxWidth, int maxHeight)
    {
        using var image = Image.Load(imageData);

        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        image.Mutate(x => x.Resize(newWidth, newHeight));

        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 90 });
        return outputStream.ToArray();
    }
}
