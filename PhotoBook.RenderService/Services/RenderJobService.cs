using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using PhotoBook.RenderService.Data;
using PhotoBook.RenderService.Models;
using static System.Net.WebRequestMethods;

namespace PhotoBook.RenderService.Services;

public interface IRenderJobService
{
    Task<List<RenderJob>> GetUserJobsAsync(Guid userId, int skip, int take);
    Task<bool> CancelJobAsync(Guid jobId, Guid userId);
    Task<RenderResponse?> RetryJobAsync(Guid jobId, Guid userId);
    Task<bool> DeleteJobAsync(Guid jobId, Guid userId);
}

public class RenderJobService : IRenderJobService
{
    private readonly RenderDbContext _context;
    private readonly BlobServiceClient _blobClient;
    private readonly IPdfRenderService _renderService;
    private readonly ILogger<RenderJobService> _logger;

    public RenderJobService(RenderDbContext context, BlobServiceClient blobClient, IPdfRenderService renderService, ILogger<RenderJobService> logger)
    {
        _context = context;
        _blobClient = blobClient;
        _renderService = renderService;
        _logger = logger;
    }

    public async Task<List<RenderJob>> GetUserJobsAsync(Guid userId, int skip, int take)
    {
        return await _context.RenderJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<bool> CancelJobAsync(Guid jobId, Guid userId)
    {
        var job = await _context.RenderJobs.FirstOrDefaultAsync(j => https://url.za.m.mimecastprotect.com/s/lar9Cr0qQGujXAXrFLHYH4jZ82?domain=j.id == jobId && j.UserId == userId);
        if (job == null || job.Status != "Processing") return false;

        job.Status = "Cancelled";
        job.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RenderResponse?> RetryJobAsync(Guid jobId, Guid userId)
    {
        var job = await _context.RenderJobs.FirstOrDefaultAsync(j => https://url.za.m.mimecastprotect.com/s/lar9Cr0qQGujXAXrFLHYH4jZ82?domain=j.id == jobId && j.UserId == userId);
        if (job == null || job.Status != "Failed") return null;

        return await _renderService.RenderPhotoBookAsync(job.PhotoBookId, job.UserId, job.Quality);
    }

    public async Task<bool> DeleteJobAsync(Guid jobId, Guid userId)
    {
        var job = await _context.RenderJobs.FirstOrDefaultAsync(j => https://url.za.m.mimecastprotect.com/s/lar9Cr0qQGujXAXrFLHYH4jZ82?domain=j.id == jobId && j.UserId == userId);
        if (job == null) return false;

        if (!string.IsNullOrEmpty(job.PdfBlobUrl))
        {
            var uri = new Uri(job.PdfBlobUrl);
            var blobName = string.Join("/", uri.Segments.Skip(2).Select(s => s.Trim('/')));
            var containerClient = _blobClient.GetBlobContainerClient("renders");
            await containerClient.DeleteBlobIfExistsAsync(blobName);
        }

        _context.RenderJobs.Remove(job);
        await _context.SaveChangesAsync();
        return true;
    }
}
