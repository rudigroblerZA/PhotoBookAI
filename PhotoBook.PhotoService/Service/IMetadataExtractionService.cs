namespace PhotoBook.PhotoService.Services;

public interface IMetadataExtractionService
{
    Task<(int width, int height, DateTime? dateTaken, string? location, int orientation)>
        ExtractMetadataAsync(Stream imageStream);
}
