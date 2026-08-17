using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.Tests.TestDoubles;

internal sealed class ThrowingSubscriber<T> : ISubscriber<T>
{
    public void OnMessage(T message) => throw new InvalidOperationException("Simulated subscriber failure.");
}
