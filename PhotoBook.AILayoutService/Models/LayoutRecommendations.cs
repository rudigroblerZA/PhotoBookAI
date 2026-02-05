namespace PhotoBook.AILayoutService.Models;

public class LayoutRecommendations
{
    public int TotalPhotos { get; set; }
    public int PortraitCount { get; set; }
    public int LandscapeCount { get; set; }
    public int SquareCount { get; set; }
    public int RecommendedPages { get; set; }
    public string RecommendedStyle { get; set; } = string.Empty;
    public string RecommendedTheme { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
}
