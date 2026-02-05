using PhotoBook.DesignService.Models;

namespace PhotoBook.DesignService.Services;

public interface IFilterService
{
    List<FilterDefinition> GetAvailableFilters();
}

public class FilterService : IFilterService
{
    public List<FilterDefinition> GetAvailableFilters()
    {
        return new List<FilterDefinition>
        {
            new FilterDefinition(
                "none",
                "No Filter",
                "Original image with no modifications",
                new Dictionary<string, float>()
            ),
            new FilterDefinition(
                "grayscale",
                "Black & White",
                "Classic monochrome filter",
                new Dictionary<string, float>()
            ),
            new FilterDefinition(
                "sepia",
                "Sepia",
                "Warm vintage tone",
                new Dictionary<string, float> { { "intensity", 0.8f } }
            ),
            new FilterDefinition(
                "brightness",
                "Brighten",
                "Increase brightness",
                new Dictionary<string, float> { { "amount", 1.2f } }
            ),
            new FilterDefinition(
                "contrast",
                "High Contrast",
                "Boost contrast for dramatic effect",
                new Dictionary<string, float> { { "amount", 1.3f } }
            ),
            new FilterDefinition(
                "vintage",
                "Vintage",
                "Retro film look with vignette",
                new Dictionary<string, float>
                {
                    { "vignette", 0.6f },
                    { "fade", 0.3f }
                }
            ),
            new FilterDefinition(
                "vivid",
                "Vivid",
                "Saturated colors and sharp details",
                new Dictionary<string, float> { { "saturation", 1.4f } }
            ),
            new FilterDefinition(
                "fade",
                "Faded",
                "Soft, washed-out aesthetic",
                new Dictionary<string, float> { { "fade", 0.5f } }
            )
        };
    }
}
