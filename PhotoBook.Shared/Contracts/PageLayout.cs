namespace PhotoBook.Shared.Contracts;

public record PageLayout(
    int PageNumber,
    string LayoutType,
    List<PhotoSlot> PhotoSlots
);
