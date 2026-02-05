namespace PhotoBook.Shared.Contracts;

public record PhotoSlot(
    int SlotIndex,
    Guid? SuggestedPhotoId,
    double X,
    double Y,
    double Width,
    double Height
);
