namespace PhotoBook.Shared.Models;

public class PhotoBook
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = "My Photo Book";
    public int PageCount { get; set; }
    public string Theme { get; set; } = "Classic";
    public List<BookPage> Pages { get; set; } = new();
    public BookStatus Status { get; set; }
    //public DateTime CreatedAt { get; set; }
}
