namespace PubSubDemo.Core.Abstractions;

/// <summary>
/// Registration side of the broker. Kept separate from <see cref="IPublisher{T}"/>
/// so that producers can depend on "I can publish" without also being able to
/// enumerate or manage subscribers, and vice versa (interface segregation).
/// </summary>
public interface ISubscribable<T>
{
    /// <summary>
    /// Registers a subscriber to receive future messages. Dispose the
    /// returned handle to unsubscribe.
    /// </summary>
    IDisposable Subscribe(ISubscriber<T> subscriber);
}
