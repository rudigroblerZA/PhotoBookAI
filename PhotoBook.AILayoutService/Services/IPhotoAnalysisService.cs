using PhotoBook.AILayoutService.Models;

namespace PhotoBook.AILayoutService.Services;

public interface IPhotoAnalysisService
{
    Task<List<PhotoAnalysisResult>> AnalyzePhotosAsync(List<Guid> photoIds);
}
