namespace PhotoBook.Shared.Models;

public class PageElement
{
    public string Type { get; set; } = string.Empty; // "photo", "text"
    public Guid? PhotoId { get; set; }
    public string? TextContent { get; set; }
    public PositionData Position { get; set; } = new();
}
