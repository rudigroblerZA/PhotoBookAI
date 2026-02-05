namespace PhotoBook.Shared.Contracts;

public record LayoutSuggestion(
    string TemplateId,
    string Theme,
    string Description,
    int PageCount,
    double ConfidenceScore,
    List<PageLayout> Pages
);
