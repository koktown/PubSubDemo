using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.Tests.TestDoubles;

internal sealed class RecordingSubscriber<T> : ISubscriber<T>
{
    private readonly List<T> _received = new();

    public IReadOnlyList<T> Received => _received;

    public void OnMessage(T message) => _received.Add(message);
}
