namespace PhotoBook.Shared.Models;

public class PhotoMetadata
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    //public DateTime? DateTaken { get; set; }
    public string? Location { get; set; }
    //public DateTime UploadedAt { get; set; }
}
