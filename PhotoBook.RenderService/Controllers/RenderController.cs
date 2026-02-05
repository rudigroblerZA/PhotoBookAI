using Microsoft.AspNetCore.Mvc;
using PhotoBook.RenderService.Services;
using PhotoBook.RenderService.Models;

namespace PhotoBook.RenderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RenderController : ControllerBase
{
    private readonly IPdfRenderService _renderService;
    private readonly IRenderJobService _jobService;
    private readonly ILogger<RenderController> _logger;

    public RenderController(
        IPdfRenderService renderService,
        IRenderJobService jobService,
        ILogger<RenderController> logger)
    {
        _renderService = renderService;
        _jobService = jobService;
        _logger = logger;
    }

    [HttpPost("render")]
    public async Task<ActionResult<RenderResponse>> RenderPhotoBook([FromBody] RenderRequest request)
    {
        _logger.LogInformation("Render request for photobook {PhotoBookId}", request.PhotoBookId);

        if (request.PhotoBookId == Guid.Empty || request.UserId == Guid.Empty)
            return BadRequest(new { error = "Invalid IDs" });

        try
        {
            var result = await _renderService.RenderPhotoBookAsync(
                request.PhotoBookId, request.UserId, request.Quality);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting render job");
            return StatusCode(500, new { error = "Failed to start render", details = ex.Message });
        }
    }

    [HttpGet("status/{jobId}")]
    public async Task<ActionResult<RenderStatus>> GetRenderStatus(Guid jobId)
    {
        var status = await _renderService.GetRenderStatusAsync(jobId);
        if (status == null)
            return NotFound(new { error = "Job not found" });
        return Ok(status);
    }

    [HttpGet("user/{userId}/jobs")]
    public async Task<ActionResult<List<RenderJob>>> GetUserJobs(Guid userId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var jobs = await _jobService.GetUserJobsAsync(userId, skip, take);
        return Ok(new { jobs, count = jobs.Count });
    }

    [HttpPost("{jobId}/cancel")]
    public async Task<IActionResult> CancelJob(Guid jobId, [FromQuery] Guid userId)
    {
        var cancelled = await _jobService.CancelJobAsync(jobId, userId);
        if (!cancelled)
            return NotFound(new { error = "Job not found or cannot be cancelled" });
        return NoContent();
    }

    [HttpPost("{jobId}/retry")]
    public async Task<ActionResult<RenderResponse>> RetryJob(Guid jobId, [FromQuery] Guid userId)
    {
        var result = await _jobService.RetryJobAsync(jobId, userId);
        if (result == null)
            return NotFound(new { error = "Job not found" });
        return Ok(result);
    }

    [HttpDelete("{jobId}")]
    public async Task<IActionResult> DeleteJob(Guid jobId, [FromQuery] Guid userId)
    {
        var deleted = await _jobService.DeleteJobAsync(jobId, userId);
        if (!deleted)
            return NotFound(new { error = "Job not found" });
        return NoContent();
    }

    [HttpPost("preview-page")]
    public async Task<IActionResult> RenderPagePreview([FromBody] PagePreviewRequest request)
    {
        try
        {
            var imageBytes = await _renderService.RenderPagePreviewAsync(request.PhotoBookId, request.PageNumber);
            return File(imageBytes, "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering preview");
            return StatusCode(500, new { error = "Failed to render preview" });
        }
    }
}
