using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.Tests.TestDoubles;

internal sealed class RecordingPublisher<T> : IPublisher<T>
{
    private readonly List<T> _published = new();

    public IReadOnlyList<T> Published => _published;

    public void Publish(T message) => _published.Add(message);
}
