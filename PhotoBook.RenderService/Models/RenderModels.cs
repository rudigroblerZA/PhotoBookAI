namespace PhotoBook.RenderService.Models;

public record RenderRequest(
    Guid PhotoBookId,
    Guid UserId,
    RenderQuality Quality = RenderQuality.High
);

public record RenderResponse(
    Guid JobId,
    string Status,
    string? PdfUrl = null,
    DateTime StartedAt = default
);

public record RenderStatus(
    Guid JobId,
    string Status,
    int Progress,
    int TotalPages,
    string? PdfUrl,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record PagePreviewRequest(
    Guid PhotoBookId,
    int PageNumber
);

public enum RenderQuality
{
    Draft = 72,
    Standard = 150,
    High = 300,
    Premium = 600
}

public class RenderJob
{
    public Guid Id { get; set; }
    public Guid PhotoBookId { get; set; }
    public Guid UserId { get; set; }
    public RenderQuality Quality { get; set; }
    public string Status { get; set; } = "Pending";
    public int Progress { get; set; }
    public int TotalPages { get; set; }
    public string? PdfBlobUrl { get; set; }
    public long? PdfFileSize { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ThemeData
{
    public string PrimaryColor { get; set; } = "#000000";
    public string SecondaryColor { get; set; } = "#FFFFFF";
    public string AccentColor { get; set; } = "#FF0000";
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 14;
    public string BorderStyle { get; set; } = "none";
    public int BorderWidth { get; set; } = 0;
    public string BorderColor { get; set; } = "#000000";
    public string BackgroundColor { get; set; } = "#FFFFFF";
}
