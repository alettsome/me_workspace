using System.Threading.Channels;

namespace me_workspace.Web.Features.Pipeline;

public sealed class PipelineQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _channel =
        Channel.CreateUnbounded<Func<CancellationToken, ValueTask>>();

    public ValueTask QueueAsync(Func<CancellationToken, ValueTask> workItem, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(workItem, cancellationToken);

    public IAsyncEnumerable<Func<CancellationToken, ValueTask>> DequeueAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
