using PhotoBook.PhotoService.Models;

namespace PhotoBook.PhotoService.Services;

public interface IPhotoUploadService
{
    Task<PhotoMetadata> UploadPhotoAsync(IFormFile file, Guid userId);
    Task<List<PhotoMetadata>> GetUserPhotosAsync(Guid userId, int skip, int take, string? sortBy, bool descending);
    Task<PhotoMetadata?> GetPhotoAsync(Guid photoId);
    Task<bool> DeletePhotoAsync(Guid photoId, Guid userId);
    Task<object> GetUserStatsAsync(Guid userId);
}
