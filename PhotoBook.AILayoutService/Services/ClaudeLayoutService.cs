using System.Text;
using System.Text.Json;
using PhotoBook.AILayoutService.Models;
using PhotoBook.Shared.Contracts;

namespace PhotoBook.AILayoutService.Services;

public class ClaudeLayoutService : ILayoutGeneratorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPhotoAnalysisService _photoAnalysis;
    private readonly ILayoutScoringService _scoringService;
    private readonly IConfiguration _config;
    private readonly ILogger<ClaudeLayoutService> _logger;

    public ClaudeLayoutService(
        IHttpClientFactory httpClientFactory,
        IPhotoAnalysisService photoAnalysis,
        ILayoutScoringService scoringService,
        IConfiguration config,
        ILogger<ClaudeLayoutService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _photoAnalysis = photoAnalysis;
        _scoringService = scoringService;
        _config = config;
        _logger = logger;
    }

    public async Task<List<LayoutSuggestion>> GenerateLayoutSuggestionsAsync(
        LayoutSuggestionRequest request)
    {
        // Analyze photos first
        var photoAnalyses = await _photoAnalysis.AnalyzePhotosAsync(request.PhotoIds);

        // Build prompt for Claude
        var prompt = BuildLayoutPrompt(request, photoAnalyses);

        // Call Claude API
        var claudeResponse = await CallClaudeAPIAsync(prompt);

        // Parse and score suggestions
        var suggestions = ParseClaudeResponse(claudeResponse, request, photoAnalyses);

        // Score each suggestion
        // TODO
        //foreach (var suggestion in suggestions)
        //{
        //    suggestion = suggestion with
        //    {
        //        ConfidenceScore = _scoringService.ScoreSuggestion(suggestion, photoAnalyses)
        //    };
        //}

        // Sort by confidence score
        return suggestions.OrderByDescending(s => s.ConfidenceScore).ToList();
    }

    public async Task<LayoutSuggestion> GenerateSmartLayoutAsync(
        List<Guid> photoIds,
        Guid userId,
        string style,
        int desiredPageCount)
    {
        var request = new LayoutSuggestionRequest(
            userId,
            photoIds,
            desiredPageCount,
            null
        );

        var suggestions = await GenerateLayoutSuggestionsAsync(request);

        // Return best matching style or highest scored
        var bestSuggestion = suggestions
            .Where(s => string.IsNullOrEmpty(style) ||
                       s.Theme.ToLower().Contains(style.ToLower()))
            .FirstOrDefault() ?? suggestions.First();

        return bestSuggestion;
    }

    public async Task<LayoutRecommendations> GetLayoutRecommendationsAsync(List<Guid> photoIds)
    {
        var analyses = await _photoAnalysis.AnalyzePhotosAsync(photoIds);

        var portraitCount = analyses.Count(a => a.Orientation == "portrait");
        var landscapeCount = analyses.Count(a => a.Orientation == "landscape");
        var squareCount = analyses.Count(a => a.Orientation == "square");

        var recommendations = new LayoutRecommendations
        {
            TotalPhotos = photoIds.Count,
            PortraitCount = portraitCount,
            LandscapeCount = landscapeCount,
            SquareCount = squareCount,
            RecommendedPages = CalculateOptimalPageCount(photoIds.Count),
            RecommendedStyle = DetermineRecommendedStyle(portraitCount, landscapeCount, squareCount),
            RecommendedTheme = DetermineRecommendedTheme(analyses),
            Suggestions = GenerateSuggestions(analyses)
        };

        return recommendations;
    }

    private string BuildLayoutPrompt(
        LayoutSuggestionRequest request,
        List<PhotoAnalysisResult> analyses)
    {
        var photoInfo = analyses.Select(a => new
        {
            id = a.PhotoId,
            orientation = a.Orientation,
            aspectRatio = a.AspectRatio,
            date = a.DateTaken?.ToString("yyyy-MM-dd"),
            hasFaces = a.HasFaces,
            quality = a.Quality
        });

        // TODO : {photoInfo.Count}

        return $@"You are a professional photo book designer with expertise in layout composition and visual storytelling.

Task: Generate 3 diverse layout suggestions for a photo book with the following requirements:

Photos: 10 total
- Portrait: {analyses.Count(a => a.Orientation == "portrait")}
- Landscape: {analyses.Count(a => a.Orientation == "landscape")}
- Square: {analyses.Count(a => a.Orientation == "square")}

Desired pages: {request.DesiredPageCount}
Theme preference: {request.PreferredTheme ?? "auto-detect"}

Photo details:
{JsonSerializer.Serialize(photoInfo, new JsonSerializerOptions { WriteIndented = true })}

Guidelines:
- Consider photo orientations for optimal placement
- Group photos by date/event when possible
- Balance dense and sparse pages
- Feature high-quality photos prominently
- Create visual flow and narrative

Respond with ONLY valid JSON (no markdown, no preamble):
{{
  ""suggestions"": [
    {{
      ""templateId"": ""unique-id"",
      ""theme"": ""Theme Name"",
      ""description"": ""Brief description"",
      ""pageCount"": {request.DesiredPageCount},
      ""confidenceScore"": 0.95,
      ""pages"": [
        {{
          ""pageNumber"": 1,
          ""layoutType"": ""hero"",
          ""photoSlots"": [
            {{
              ""slotIndex"": 0,
              ""suggestedPhotoId"": ""{photoInfo.First().id}"",
              ""x"": 0.1,
              ""y"": 0.1,
              ""width"": 0.8,
              ""height"": 0.8
            }}
          ]
        }}
      ]
    }}
  ]
}}";
    }

    private async Task<string> CallClaudeAPIAsync(string prompt)
    {
        var apiKey = _config["Anthropic:ApiKey"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("No Claude API key found, using fallback layouts");
            return GenerateFallbackResponse();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ClaudeAPI");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var requestBody = new
            {
                model = "claude-sonnet-4-20250514",
                max_tokens = 4000,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/messages", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(result);

            if (claudeResponse?.Content != null && claudeResponse.Content.Count > 0)
            {
                return claudeResponse.Content[0].Text ?? GenerateFallbackResponse();
            }

            return GenerateFallbackResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
            return GenerateFallbackResponse();
        }
    }

    private List<LayoutSuggestion> ParseClaudeResponse(
        string response,
        LayoutSuggestionRequest request,
        List<PhotoAnalysisResult> analyses)
    {
        try
        {
            // Clean response (remove markdown code blocks if present)
            var cleaned = response.Trim();
            // TODO
            //if (cleaned.StartsWith("```"))
            //{
            //    cleaned = cleaned.Split('\n').Skip(1).ToArray();
            //    cleaned = string.Join('\n', cleaned.Take(cleaned.Length - 1));
            //}

            var parsed = JsonSerializer.Deserialize<LayoutSuggestionsResponse>(cleaned);

            if (parsed?.Suggestions != null && parsed.Suggestions.Count > 0)
            {
                return parsed.Suggestions;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Claude response, using fallback");
        }

        // Fallback to rule-based layouts
        return GenerateFallbackLayouts(request, analyses);
    }

    private string GenerateFallbackResponse()
    {
        return @"{""suggestions"":[]}";
    }

    private List<LayoutSuggestion> GenerateFallbackLayouts(
        LayoutSuggestionRequest request,
        List<PhotoAnalysisResult> analyses)
    {
        var suggestions = new List<LayoutSuggestion>();

        // Chronological layout
        suggestions.Add(GenerateChronologicalLayout(request, analyses));

        // Grid layout
        suggestions.Add(GenerateGridLayout(request, analyses));

        // Mixed layout
        suggestions.Add(GenerateMixedLayout(request, analyses));

        return suggestions;
    }

    private LayoutSuggestion GenerateChronologicalLayout(
        LayoutSuggestionRequest request,
        List<PhotoAnalysisResult> analyses)
    {
        var pages = new List<PageLayout>();
        var sortedPhotos = analyses.OrderBy(a => a.DateTaken ?? DateTime.MinValue).ToList();

        int photosPerPage = 2;
        int pageNumber = 1;

        for (int i = 0; i < sortedPhotos.Count; i += photosPerPage)
        {
            var pagePhotos = sortedPhotos.Skip(i).Take(photosPerPage).ToList();
            var slots = new List<PhotoSlot>();

            for (int j = 0; j < pagePhotos.Count; j++)
            {
                slots.Add(new PhotoSlot(
                    j,
                    pagePhotos[j].PhotoId,
                    j == 0 ? 0.05 : 0.55,
                    0.1,
                    0.4,
                    0.8
                ));
            }

            pages.Add(new PageLayout(pageNumber, "sidebyside", slots));
            pageNumber++;
        }

        return new LayoutSuggestion(
            "chronological-story",
            "Chronological Story",
            "Photos arranged by date with 2 per page for easy viewing",
            pages.Count,
            0.85,
            pages
        );
    }

    private LayoutSuggestion GenerateGridLayout(
        LayoutSuggestionRequest request,
        List<PhotoAnalysisResult> analyses)
    {
        var pages = new List<PageLayout>();
        int photosPerPage = 4;
        int pageNumber = 1;

        for (int i = 0; i < analyses.Count; i += photosPerPage)
        {
            var pagePhotos = analyses.Skip(i).Take(photosPerPage).ToList();
            var slots = new List<PhotoSlot>();

            var positions = new[] { (0.05, 0.05), (0.55, 0.05), (0.05, 0.55), (0.55, 0.55) };

            for (int j = 0; j < pagePhotos.Count; j++)
            {
                slots.Add(new PhotoSlot(
                    j,
                    pagePhotos[j].PhotoId,
                    positions[j].Item1,
                    positions[j].Item2,
                    0.4,
                    0.4
                ));
            }

            pages.Add(new PageLayout(pageNumber, "grid", slots));
            pageNumber++;
        }

        return new LayoutSuggestion(
            "balanced-grid",
            "Balanced Grid",
            "Uniform 4-photo grid layout for comprehensive coverage",
            pages.Count,
            0.80,
            pages
        );
    }

    private LayoutSuggestion GenerateMixedLayout(
        LayoutSuggestionRequest request,
        List<PhotoAnalysisResult> analyses)
    {
        var pages = new List<PageLayout>();
        int pageNumber = 1;
        int photoIndex = 0;

        while (photoIndex < analyses.Count)
        {
            var remainingPhotos = analyses.Count - photoIndex;
            var slots = new List<PhotoSlot>();

            if (remainingPhotos >= 3 && pageNumber % 3 == 1)
            {
                // Feature layout: 1 large + 2 small
                slots.Add(new PhotoSlot(0, analyses[photoIndex].PhotoId, 0.05, 0.05, 0.6, 0.9));
                slots.Add(new PhotoSlot(1, analyses[photoIndex + 1].PhotoId, 0.7, 0.05, 0.25, 0.4));
                slots.Add(new PhotoSlot(2, analyses[photoIndex + 2].PhotoId, 0.7, 0.55, 0.25, 0.4));
                photoIndex += 3;
                pages.Add(new PageLayout(pageNumber, "feature", slots));
            }
            else if (remainingPhotos >= 2)
            {
                // Side by side
                slots.Add(new PhotoSlot(0, analyses[photoIndex].PhotoId, 0.05, 0.1, 0.4, 0.8));
                slots.Add(new PhotoSlot(1, analyses[photoIndex + 1].PhotoId, 0.55, 0.1, 0.4, 0.8));
                photoIndex += 2;
                pages.Add(new PageLayout(pageNumber, "sidebyside", slots));
            }
            else
            {
                // Single hero
                slots.Add(new PhotoSlot(0, analyses[photoIndex].PhotoId, 0.1, 0.1, 0.8, 0.8));
                photoIndex += 1;
                pages.Add(new PageLayout(pageNumber, "hero", slots));
            }

            pageNumber++;
        }

        return new LayoutSuggestion(
            "dynamic-mix",
            "Dynamic Mix",
            "Varied layouts that adapt to content and create visual interest",
            pages.Count,
            0.90,
            pages
        );
    }

    private int CalculateOptimalPageCount(int photoCount)
    {
        // Rule of thumb: 2-3 photos per page on average
        return (int)Math.Ceiling(photoCount / 2.5);
    }

    private string DetermineRecommendedStyle(int portrait, int landscape, int square)
    {
        if (landscape > portrait && landscape > square)
            return "Wide & Spacious";

        if (portrait > landscape && portrait > square)
            return "Portrait Focused";

        if (square > portrait + landscape)
            return "Grid Gallery";

        return "Mixed Layout";
    }

    private string DetermineRecommendedTheme(List<PhotoAnalysisResult> analyses)
    {
        // Analyze brightness and colors to suggest theme
        var avgBrightness = analyses.Average(a => a.Brightness);

        if (avgBrightness > 0.7)
            return "Modern";

        if (avgBrightness < 0.3)
            return "Classic";

        return "Vintage";
    }

    private List<string> GenerateSuggestions(List<PhotoAnalysisResult> analyses)
    {
        var suggestions = new List<string>();

        var lowQualityCount = analyses.Count(a => a.Quality == "low");
        if (lowQualityCount > analyses.Count * 0.2)
        {
            suggestions.Add("Consider using higher resolution photos for better print quality");
        }

        var withFaces = analyses.Count(a => a.HasFaces);
        if (withFaces > analyses.Count * 0.7)
        {
            suggestions.Add("Many photos contain people - consider a portrait-focused layout");
        }

        if (analyses.Count < 10)
        {
            suggestions.Add("For books with fewer photos, consider larger layouts to showcase each image");
        }

        return suggestions;
    }
}

// Claude API response models
public class ClaudeApiResponse
{
    public List<ClaudeContent> Content { get; set; } = new();
}

public class ClaudeContent
{
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class LayoutSuggestionsResponse
{
    public List<LayoutSuggestion> Suggestions { get; set; } = new();
}
