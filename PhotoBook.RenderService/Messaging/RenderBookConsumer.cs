using MassTransit;
using PhotoBook.RenderService.Services;
using PhotoBook.RenderService.Models;

namespace PhotoBook.RenderService.Messaging;

public record RenderBookCommand(Guid PhotoBookId, Guid UserId, RenderQuality Quality);

public class RenderBookConsumer : IConsumer<RenderBookCommand>
{
    private readonly IPdfRenderService _renderService;

    public RenderBookConsumer(IPdfRenderService renderService)
    {
        _renderService = renderService;
    }

    public async Task Consume(ConsumeContext<RenderBookCommand> context)
    {
        var command = context.Message;
        await _renderService.RenderPhotoBookAsync(command.PhotoBookId, command.UserId, command.Quality);
    }
}
