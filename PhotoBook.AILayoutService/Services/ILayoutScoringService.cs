using PhotoBook.AILayoutService.Models;
using PhotoBook.Shared.Contracts;

namespace PhotoBook.AILayoutService.Services;

public interface ILayoutScoringService
{
    double ScoreSuggestion(LayoutSuggestion suggestion, List<PhotoAnalysisResult> analyses);
}
