namespace PhotoBook.AILayoutService.Models;

public class PhotoAnalysisResult
{
    public Guid PhotoId { get; set; }
    public string Orientation { get; set; } = string.Empty; // portrait, landscape, square
    public int Width { get; set; }
    public int Height { get; set; }
    public double AspectRatio { get; set; }
    public List<string> DominantColors { get; set; } = new();
    public DateTime? DateTaken { get; set; }
    public string? Location { get; set; }
    public double Brightness { get; set; }
    public double Contrast { get; set; }
    public bool HasFaces { get; set; }
    public int FaceCount { get; set; }
    public string Quality { get; set; } = "good"; // low, medium, good, excellent
}
