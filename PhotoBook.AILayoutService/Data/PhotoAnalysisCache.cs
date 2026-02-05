namespace PhotoBook.AILayoutService.Data;

public class PhotoAnalysisCache
{
    public Guid PhotoId { get; set; }
    public string AnalysisJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
