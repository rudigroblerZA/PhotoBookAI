using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;

namespace PhotoBook.RenderService.Services;

public interface IPhotoFetchService
{
    Task<byte[]?> FetchPhotoAsync(Guid photoId);
}

public class PhotoFetchService : IPhotoFetchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly BlobServiceClient _blobClient;
    private readonly ILogger<PhotoFetchService> _logger;

    public PhotoFetchService(IHttpClientFactory httpClientFactory, IMemoryCache cache, BlobServiceClient blobClient, ILogger<PhotoFetchService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _blobClient = blobClient;
        _logger = logger;
    }

    public async Task<byte[]?> FetchPhotoAsync(Guid photoId)
    {
        var cacheKey = $"photo_{photoId}";

        if (_cache.TryGetValue(cacheKey, out byte[]? cachedPhoto))
            return cachedPhoto;

        try
        {
            var client = _httpClientFactory.CreateClient("PhotoService");
            var response = await client.GetAsync($"/api/photo/{photoId}/metadata");

            if (!response.IsSuccessStatusCode) return null;

            var photoMeta = await response.Content.ReadFromJsonAsync<PhotoMetadataResponse>();
            if (photoMeta == null) return null;

            var containerClient = _blobClient.GetBlobContainerClient("photos");
            var blobClient = containerClient.GetBlobClient(photoMeta.BlobPath);

            if (!await blobClient.ExistsAsync()) return null;

            using var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            var photoBytes = memoryStream.ToArray();

            _cache.Set(cacheKey, photoBytes, TimeSpan.FromHours(1));
            return photoBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching photo {PhotoId}", photoId);
            return null;
        }
    }

    private record PhotoMetadataResponse(string BlobPath, string BlobUrl);
}
