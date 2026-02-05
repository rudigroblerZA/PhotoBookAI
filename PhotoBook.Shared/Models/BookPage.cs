namespace PhotoBook.Shared.Models;

public class BookPage
{
    public int PageNumber { get; set; }
    public string LayoutTemplateId { get; set; } = string.Empty;
    public List<PageElement> Elements { get; set; } = new();
}
