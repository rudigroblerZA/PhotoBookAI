using Microsoft.AspNetCore.Mvc;
using PhotoBook.DesignService.Models;
using PhotoBook.DesignService.Services;

namespace PhotoBook.DesignService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DesignController : ControllerBase
{
    private readonly IDesignService _designService;
    private readonly IThemeService _themeService;
    private readonly IFilterService _filterService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<DesignController> _logger;

    public DesignController(
        IDesignService designService,
        IThemeService themeService,
        IFilterService filterService,
        ITemplateService templateService,
        ILogger<DesignController> logger)
    {
        _designService = designService;
        _themeService = themeService;
        _filterService = filterService;
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new photo book
    /// </summary>
    [HttpPost("photobook")]
    public async Task<ActionResult<PhotoBook.Shared.Models.PhotoBook>> CreatePhotoBook([FromBody] CreatePhotoBookRequest request)
    {
        _logger.LogInformation("Creating photo book for user {UserId}", request.UserId);

        if (request.UserId == Guid.Empty)
            return BadRequest(new { error = "Invalid user ID" });

        try
        {
            var photoBook = await _designService.CreatePhotoBookAsync(
                request.UserId,
                request.Title,
                request.ThemeId);

            return Ok(photoBook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating photo book");
            return StatusCode(500, new { error = "Failed to create photo book" });
        }
    }

    /// <summary>
    /// Get a photo book by ID
    /// </summary>
    [HttpGet("photobook/{id}")]
    public async Task<ActionResult<PhotoBook.Shared.Models.PhotoBook>> GetPhotoBook(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid photo book ID" });

        var photoBook = await _designService.GetPhotoBookAsync(id);
        if (photoBook == null)
            return NotFound(new { error = "Photo book not found" });

        return Ok(photoBook);
    }

    /// <summary>
    /// Get all photo books for a user
    /// </summary>
    [HttpGet("user/{userId}/photobooks")]
    public async Task<ActionResult<List<PhotoBook.Shared.Models.PhotoBook>>> GetUserPhotoBooks(
        Guid userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { error = "Invalid user ID" });

        try
        {
            var photoBooks = await _designService.GetUserPhotoBooksAsync(userId, skip, take);
            return Ok(new { photoBooks, count = photoBooks.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving photo books for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve photo books" });
        }
    }

    /// <summary>
    /// Update photo book metadata
    /// </summary>
    [HttpPut("photobook/{id}")]
    public async Task<IActionResult> UpdatePhotoBook(
        Guid id,
        [FromBody] UpdatePhotoBookRequest request)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid photo book ID" });

        try
        {
            await _designService.UpdatePhotoBookAsync(id, request.Title, request.Status);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating photo book {Id}", id);
            return StatusCode(500, new { error = "Failed to update photo book" });
        }
    }

    /// <summary>
    /// Delete a photo book
    /// </summary>
    [HttpDelete("photobook/{id}")]
    public async Task<IActionResult> DeletePhotoBook(Guid id, [FromQuery] Guid userId)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
            return BadRequest(new { error = "Invalid ID" });

        try
        {
            var deleted = await _designService.DeletePhotoBookAsync(id, userId);
            if (!deleted)
                return NotFound(new { error = "Photo book not found or access denied" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo book {Id}", id);
            return StatusCode(500, new { error = "Failed to delete photo book" });
        }
    }

    /// <summary>
    /// Apply a theme to a photo book
    /// </summary>
    [HttpPut("photobook/{id}/theme")]
    public async Task<IActionResult> ApplyTheme(Guid id, [FromBody] ApplyThemeRequest request)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid photo book ID" });

        try
        {
            await _designService.ApplyThemeAsync(id, request.ThemeId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying theme to photo book {Id}", id);
            return StatusCode(500, new { error = "Failed to apply theme" });
        }
    }

    /// <summary>
    /// Update a specific page
    /// </summary>
    [HttpPut("photobook/{id}/page/{pageNumber}")]
    public async Task<IActionResult> UpdatePage(
        Guid id,
        int pageNumber,
        [FromBody] UpdatePageRequest request)
    {
        if (id == Guid.Empty || pageNumber < 1)
            return BadRequest(new { error = "Invalid parameters" });

        try
        {
            await _designService.UpdatePageAsync(id, pageNumber, request.Elements);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating page {Page} of photo book {Id}", pageNumber, id);
            return StatusCode(500, new { error = "Failed to update page" });
        }
    }

    /// <summary>
    /// Add a text element to a page
    /// </summary>
    [HttpPost("photobook/{id}/page/{pageNumber}/text")]
    public async Task<IActionResult> AddTextElement(
        Guid id,
        int pageNumber,
        [FromBody] AddTextRequest request)
    {
        if (id == Guid.Empty || pageNumber < 1)
            return BadRequest(new { error = "Invalid parameters" });

        try
        {
            await _designService.AddTextElementAsync(
                id,
                pageNumber,
                request.Text,
                request.Position,
                request.FontFamily,
                request.FontSize,
                request.Color);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding text to page {Page} of photo book {Id}", pageNumber, id);
            return StatusCode(500, new { error = "Failed to add text" });
        }
    }

    /// <summary>
    /// Add a photo element to a page
    /// </summary>
    [HttpPost("photobook/{id}/page/{pageNumber}/photo")]
    public async Task<IActionResult> AddPhotoElement(
        Guid id,
        int pageNumber,
        [FromBody] AddPhotoRequest request)
    {
        if (id == Guid.Empty || pageNumber < 1)
            return BadRequest(new { error = "Invalid parameters" });

        try
        {
            await _designService.AddPhotoElementAsync(
                id,
                pageNumber,
                request.PhotoId,
                request.Position,
                request.FilterId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding photo to page {Page} of photo book {Id}", pageNumber, id);
            return StatusCode(500, new { error = "Failed to add photo" });
        }
    }

    /// <summary>
    /// Remove an element from a page
    /// </summary>
    [HttpDelete("photobook/{id}/page/{pageNumber}/element/{elementIndex}")]
    public async Task<IActionResult> RemovePageElement(
        Guid id,
        int pageNumber,
        int elementIndex)
    {
        if (id == Guid.Empty || pageNumber < 1 || elementIndex < 0)
            return BadRequest(new { error = "Invalid parameters" });

        try
        {
            await _designService.RemovePageElementAsync(id, pageNumber, elementIndex);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing element from page");
            return StatusCode(500, new { error = "Failed to remove element" });
        }
    }

    /// <summary>
    /// Apply a template to pages
    /// </summary>
    [HttpPost("photobook/{id}/apply-template")]
    public async Task<IActionResult> ApplyTemplate(
        Guid id,
        [FromBody] ApplyTemplateRequest request)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid photo book ID" });

        try
        {
            await _designService.ApplyTemplateAsync(
                id,
                request.TemplateId,
                request.PhotoIds,
                request.StartPage);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying template");
            return StatusCode(500, new { error = "Failed to apply template" });
        }
    }

    /// <summary>
    /// Get all available themes
    /// </summary>
    [HttpGet("themes")]
    public async Task<ActionResult<List<Data.Theme>>> GetThemes()
    {
        try
        {
            var themes = await _themeService.GetAllThemesAsync();
            return Ok(themes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving themes");
            return StatusCode(500, new { error = "Failed to retrieve themes" });
        }
    }

    /// <summary>
    /// Get a specific theme
    /// </summary>
    [HttpGet("themes/{id}")]
    public async Task<ActionResult<Data.Theme>> GetTheme(Guid id)
    {
        var theme = await _themeService.GetThemeAsync(id);
        if (theme == null)
            return NotFound(new { error = "Theme not found" });

        return Ok(theme);
    }

    /// <summary>
    /// Get all available filters
    /// </summary>
    [HttpGet("filters")]
    public ActionResult<List<FilterDefinition>> GetFilters()
    {
        var filters = _filterService.GetAvailableFilters();
        return Ok(filters);
    }

    /// <summary>
    /// Get all available templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<List<Data.DesignTemplate>>> GetTemplates(
        [FromQuery] string? category = null)
    {
        try
        {
            var templates = await _templateService.GetTemplatesAsync(category);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates");
            return StatusCode(500, new { error = "Failed to retrieve templates" });
        }
    }

    /// <summary>
    /// Generate a preview image for a page
    /// </summary>
    [HttpPost("preview")]
    public async Task<ActionResult<string>> GeneratePreview(
        [FromBody] GeneratePreviewRequest request)
    {
        try
        {
            var previewUrl = await _designService.GeneratePreviewAsync(
                request.PhotoBookId,
                request.PageNumber);

            return Ok(new { previewUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview");
            return StatusCode(500, new { error = "Failed to generate preview" });
        }
    }
}
