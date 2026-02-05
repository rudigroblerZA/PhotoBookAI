namespace PhotoBook.PhotoService.Models;

public class PhotoAlbum
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Guid> PhotoIds { get; set; } = new();
}
