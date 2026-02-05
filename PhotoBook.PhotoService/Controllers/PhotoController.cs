using Microsoft.AspNetCore.Mvc;
using PhotoBook.PhotoService.Models;
using PhotoBook.PhotoService.Services;

namespace PhotoBook.PhotoService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhotoController : ControllerBase
{
    private readonly IPhotoUploadService _uploadService;
    private readonly ILogger<PhotoController> _logger;

    public PhotoController(
        IPhotoUploadService uploadService,
        ILogger<PhotoController> logger)
    {
        _uploadService = uploadService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single photo
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(104857600)] // 100MB
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
    public async Task<ActionResult<PhotoMetadata>> UploadPhoto(
        [FromForm] IFormFile file,
        [FromForm] Guid userId)
    {
        _logger.LogInformation("Upload request received for user {UserId}", userId);

        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        if (userId == Guid.Empty)
            //userId = Guid.NewGuid();
            return BadRequest(new { error = "Invalid user ID" });

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".heic", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { error = $"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}" });

        // Validate file size (100MB max)
        if (file.Length > 104857600)
            return BadRequest(new { error = "File size exceeds 100MB limit" });

        try
        {
            var photo = await _uploadService.UploadPhotoAsync(file, userId);
            _logger.LogInformation("Photo uploaded successfully: {PhotoId}", photo.Id);
            return Ok(photo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to upload photo", details = ex.Message });
        }
    }

    /// <summary>
    /// Upload multiple photos
    /// </summary>
    [HttpPost("upload-batch")]
    [RequestSizeLimit(524288000)] // 500MB total
    public async Task<ActionResult<List<PhotoMetadata>>> UploadPhotos(
        [FromForm] List<IFormFile> files,
        [FromForm] Guid userId)
    {
        _logger.LogInformation("Batch upload request received: {Count} files for user {UserId}",
            files.Count, userId);

        if (files == null || files.Count == 0)
            return BadRequest(new { error = "No files provided" });

        if (files.Count > 100)
            return BadRequest(new { error = "Maximum 100 files per batch" });

        if (userId == Guid.Empty)
            return BadRequest(new { error = "Invalid user ID" });

        var uploadedPhotos = new List<PhotoMetadata>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            try
            {
                var photo = await _uploadService.UploadPhotoAsync(file, userId);
                uploadedPhotos.Add(photo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                errors.Add($"{file.FileName}: {ex.Message}");
            }
        }

        return Ok(new
        {
            uploaded = uploadedPhotos,
            errors = errors.Count > 0 ? errors : null,
            total = files.Count,
            successful = uploadedPhotos.Count
        });
    }

    /// <summary>
    /// Get all photos for a user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<PhotoMetadata>>> GetUserPhotos(
        Guid userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? sortBy = "uploadedAt",
        [FromQuery] bool descending = true)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { error = "Invalid user ID" });

        if (take > 200)
            return BadRequest(new { error = "Maximum 200 photos per request" });

        try
        {
            var photos = await _uploadService.GetUserPhotosAsync(
                userId, skip, take, sortBy, descending);

            return Ok(new
            {
                photos,
                count = photos.Count,
                skip,
                take
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving photos for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve photos" });
        }
    }

    /// <summary>
    /// Get a specific photo by ID
    /// </summary>
    [HttpGet("{photoId}")]
    public async Task<ActionResult<PhotoMetadata>> GetPhoto(Guid photoId)
    {
        if (photoId == Guid.Empty)
            return BadRequest(new { error = "Invalid photo ID" });

        try
        {
            var photo = await _uploadService.GetPhotoAsync(photoId);
            if (photo == null)
                return NotFound(new { error = "Photo not found" });

            return Ok(photo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving photo {PhotoId}", photoId);
            return StatusCode(500, new { error = "Failed to retrieve photo" });
        }
    }

    /// <summary>
    /// Get photo metadata (used by RenderService)
    /// </summary>
    [HttpGet("{photoId}/metadata")]
    public async Task<ActionResult<object>> GetPhotoMetadata(Guid photoId)
    {
        var photo = await _uploadService.GetPhotoAsync(photoId);
        if (photo == null)
            return NotFound();

        var blobPath = $"{photo.UserId}/{photo.Id}{Path.GetExtension(photo.FileName)}";

        return Ok(new
        {
            blobPath,
            blobUrl = photo.BlobUrl,
            fileName = photo.FileName,
            width = photo.Width,
            height = photo.Height
        });
    }

    /// <summary>
    /// Delete a photo
    /// </summary>
    [HttpDelete("{photoId}")]
    public async Task<IActionResult> DeletePhoto(Guid photoId, [FromQuery] Guid userId)
    {
        if (photoId == Guid.Empty || userId == Guid.Empty)
            return BadRequest(new { error = "Invalid photo or user ID" });

        try
        {
            var deleted = await _uploadService.DeletePhotoAsync(photoId, userId);
            if (!deleted)
                return NotFound(new { error = "Photo not found or access denied" });

            _logger.LogInformation("Photo deleted: {PhotoId} by user {UserId}", photoId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo {PhotoId}", photoId);
            return StatusCode(500, new { error = "Failed to delete photo" });
        }
    }

    /// <summary>
    /// Get photo statistics for a user
    /// </summary>
    [HttpGet("user/{userId}/stats")]
    public async Task<ActionResult<object>> GetUserStats(Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { error = "Invalid user ID" });

        try
        {
            var stats = await _uploadService.GetUserStatsAsync(userId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stats for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve stats" });
        }
    }
}
