using Microsoft.AspNetCore.Mvc;
using PhotoBook.AILayoutService.Models;
using PhotoBook.AILayoutService.Services;
using PhotoBook.Shared.Contracts;

namespace PhotoBook.AILayoutService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LayoutController : ControllerBase
{
    private readonly ILayoutGeneratorService _layoutGenerator;
    private readonly IPhotoAnalysisService _photoAnalysis;
    private readonly ILayoutCacheService _cacheService;
    private readonly ILogger<LayoutController> _logger;

    public LayoutController(
        ILayoutGeneratorService layoutGenerator,
        IPhotoAnalysisService photoAnalysis,
        ILayoutCacheService cacheService,
        ILogger<LayoutController> logger)
    {
        _layoutGenerator = layoutGenerator;
        _photoAnalysis = photoAnalysis;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Generate AI-powered layout suggestions
    /// </summary>
    [HttpPost("suggest")]
    public async Task<ActionResult<List<LayoutSuggestion>>> GenerateSuggestions(
        [FromBody] LayoutSuggestionRequest request)
    {
        _logger.LogInformation("Layout suggestion request for user {UserId} with {PhotoCount} photos",
            request.UserId, request.PhotoIds.Count);

        if (request.PhotoIds.Count == 0)
            return BadRequest(new { error = "At least one photo is required" });

        if (request.PhotoIds.Count > 100)
            return BadRequest(new { error = "Maximum 100 photos per request" });

        if (request.DesiredPageCount < 1 || request.DesiredPageCount > 200)
            return BadRequest(new { error = "Page count must be between 1 and 200" });

        try
        {
            // Check cache first
            var cacheKey = _cacheService.GenerateCacheKey(request);
            var cached = await _cacheService.GetCachedSuggestionsAsync(cacheKey);

            if (cached != null)
            {
                _logger.LogInformation("Returning cached layout suggestions");
                return Ok(cached);
            }

            // Generate new suggestions
            var suggestions = await _layoutGenerator.GenerateLayoutSuggestionsAsync(request);

            // Cache for future requests
            await _cacheService.CacheSuggestionsAsync(cacheKey, suggestions);

            _logger.LogInformation("Generated {Count} layout suggestions", suggestions.Count);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating layout suggestions");
            return StatusCode(500, new { error = "Failed to generate suggestions", details = ex.Message });
        }
    }

    /// <summary>
    /// Analyze photos to extract features for layout generation
    /// </summary>
    [HttpPost("analyze-photos")]
    public async Task<ActionResult<List<PhotoAnalysisResult>>> AnalyzePhotos(
        [FromBody] AnalyzePhotosRequest request)
    {
        _logger.LogInformation("Photo analysis request for {PhotoCount} photos", request.PhotoIds.Count);

        if (request.PhotoIds.Count == 0)
            return BadRequest(new { error = "At least one photo is required" });

        try
        {
            var analyses = await _photoAnalysis.AnalyzePhotosAsync(request.PhotoIds);
            return Ok(analyses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing photos");
            return StatusCode(500, new { error = "Failed to analyze photos" });
        }
    }

    /// <summary>
    /// Generate a smart layout based on photo characteristics
    /// </summary>
    [HttpPost("smart-layout")]
    public async Task<ActionResult<LayoutSuggestion>> GenerateSmartLayout(
        [FromBody] SmartLayoutRequest request)
    {
        _logger.LogInformation("Smart layout request for {PhotoCount} photos", request.PhotoIds.Count);

        if (request.PhotoIds.Count == 0)
            return BadRequest(new { error = "At least one photo is required" });

        try
        {
            var suggestion = await _layoutGenerator.GenerateSmartLayoutAsync(
                request.PhotoIds,
                request.UserId,
                request.Style,
                request.DesiredPageCount);

            return Ok(suggestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating smart layout");
            return StatusCode(500, new { error = "Failed to generate smart layout" });
        }
    }

    /// <summary>
    /// Get layout statistics and recommendations
    /// </summary>
    [HttpPost("recommendations")]
    public async Task<ActionResult<LayoutRecommendations>> GetRecommendations(
        [FromBody] List<Guid> photoIds)
    {
        if (photoIds.Count == 0)
            return BadRequest(new { error = "At least one photo is required" });

        try
        {
            var recommendations = await _layoutGenerator.GetLayoutRecommendationsAsync(photoIds);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations");
            return StatusCode(500, new { error = "Failed to get recommendations" });
        }
    }
}
