namespace PhotoBook.Shared.Contracts;

public record LayoutSuggestionRequest(
    Guid UserId,
    List<Guid> PhotoIds,
    int DesiredPageCount,
    string? PreferredTheme
);
