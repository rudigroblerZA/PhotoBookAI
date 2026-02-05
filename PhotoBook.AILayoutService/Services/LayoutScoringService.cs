using PhotoBook.AILayoutService.Models;
using PhotoBook.Shared.Contracts;

namespace PhotoBook.AILayoutService.Services;

public class LayoutScoringService : ILayoutScoringService
{
    public double ScoreSuggestion(LayoutSuggestion suggestion, List<PhotoAnalysisResult> analyses)
    {
        double score = 0.5; // Base score

        // Score based on layout quality
        score += ScoreLayoutBalance(suggestion) * 0.2;
        score += ScorePhotoPlacement(suggestion, analyses) * 0.3;
        score += ScorePageCount(suggestion.PageCount, analyses.Count) * 0.2;
        score += ScoreVariety(suggestion) * 0.15;
        score += ScorePhotoQuality(suggestion, analyses) * 0.15;

        return Math.Min(1.0, Math.Max(0.0, score));
    }

    private double ScoreLayoutBalance(LayoutSuggestion suggestion)
    {
        // Check if photos are evenly distributed across pages
        var photosPerPage = suggestion.Pages
            .Select(p => p.PhotoSlots.Count)
            .ToList();

        if (photosPerPage.Count == 0)
            return 0.0;

        var avgPhotosPerPage = photosPerPage.Average();
        var variance = photosPerPage
            .Select(count => Math.Pow(count - avgPhotosPerPage, 2))
            .Average();

        // Lower variance = better balance
        return Math.Max(0, 1.0 - (variance / 10.0));
    }

    private double ScorePhotoPlacement(
        LayoutSuggestion suggestion,
        List<PhotoAnalysisResult> analyses)
    {
        double score = 0.0;
        int matchCount = 0;

        foreach (var page in suggestion.Pages)
        {
            foreach (var slot in page.PhotoSlots)
            {
                if (!slot.SuggestedPhotoId.HasValue)
                    continue;

                var analysis = analyses.FirstOrDefault(a => a.PhotoId == slot.SuggestedPhotoId);
                if (analysis == null)
                    continue;

                // Check if orientation matches slot dimensions
                var slotIsWide = slot.Width > slot.Height;
                var photoIsWide = analysis.Orientation == "landscape";

                if (slotIsWide == photoIsWide)
                    score += 1.0;

                matchCount++;
            }
        }

        return matchCount > 0 ? score / matchCount : 0.5;
    }

    private double ScorePageCount(int pageCount, int photoCount)
    {
        // Ideal: 2-3 photos per page
        var idealPages = photoCount / 2.5;
        var difference = Math.Abs(pageCount - idealPages);

        return Math.Max(0, 1.0 - (difference / idealPages * 0.5));
    }

    private double ScoreVariety(LayoutSuggestion suggestion)
    {
        // Reward layouts with varied page types
        var layoutTypes = suggestion.Pages
            .Select(p => p.LayoutType)
            .Distinct()
            .Count();

        return Math.Min(1.0, layoutTypes / 4.0);
    }

    private double ScorePhotoQuality(
        LayoutSuggestion suggestion,
        List<PhotoAnalysisResult> analyses)
    {
        var qualityScores = new Dictionary<string, double>
        {
            ["excellent"] = 1.0,
            ["good"] = 0.8,
            ["medium"] = 0.6,
            ["low"] = 0.3
        };

        double totalScore = 0;
        int count = 0;

        foreach (var page in suggestion.Pages)
        {
            foreach (var slot in page.PhotoSlots)
            {
                if (!slot.SuggestedPhotoId.HasValue)
                    continue;

                var analysis = analyses.FirstOrDefault(a => a.PhotoId == slot.SuggestedPhotoId);
                if (analysis != null && qualityScores.ContainsKey(analysis.Quality))
                {
                    totalScore += qualityScores[analysis.Quality];
                    count++;
                }
            }
        }

        return count > 0 ? totalScore / count : 0.5;
    }
}
