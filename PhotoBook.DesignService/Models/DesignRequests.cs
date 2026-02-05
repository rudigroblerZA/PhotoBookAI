using PhotoBook.Shared.Models;

namespace PhotoBook.DesignService.Models;

public record CreatePhotoBookRequest(
    Guid UserId,
    string Title,
    Guid? ThemeId = null
);

public record UpdatePhotoBookRequest(
    string? Title = null,
    BookStatus? Status = null
);

public record ApplyThemeRequest(Guid ThemeId);

public record UpdatePageRequest(List<PageElement> Elements);

public record AddTextRequest(
    string Text,
    PositionData Position,
    string? FontFamily = null,
    int? FontSize = null,
    string? Color = null
);

public record AddPhotoRequest(
    Guid PhotoId,
    PositionData Position,
    string? FilterId = null
);

public record ApplyTemplateRequest(
    Guid TemplateId,
    List<Guid> PhotoIds,
    int StartPage = 1
);

public record GeneratePreviewRequest(
    Guid PhotoBookId,
    int PageNumber
);

public record FilterDefinition(
    string Id,
    string Name,
    string Description,
    Dictionary<string, float> Parameters
);
