namespace PhotoBook.PhotoService.Services;

public interface IImageProcessingService
{
    Task<string> GenerateThumbnailAsync(Stream imageStream, Guid userId, Guid photoId, string extension);
    Task<byte[]> ApplyFilterAsync(byte[] imageData, string filterId);
    Task<byte[]> ResizeImageAsync(byte[] imageData, int maxWidth, int maxHeight);
}
