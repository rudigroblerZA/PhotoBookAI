namespace PhotoBook.AILayoutService.Models;

public record SmartLayoutRequest(
    Guid UserId,
    List<Guid> PhotoIds,
    string Style,
    int DesiredPageCount
);
