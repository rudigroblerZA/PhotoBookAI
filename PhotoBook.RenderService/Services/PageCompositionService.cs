namespace PhotoBook.RenderService.Services;

public interface IPageCompositionService
{
    Task<byte[]> ComposePageAsync(string pageJson);
}

public class PageCompositionService : IPageCompositionService
{
    public async Task<byte[]> ComposePageAsync(string pageJson)
    {
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }
}
