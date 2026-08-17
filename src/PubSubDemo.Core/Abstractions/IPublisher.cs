namespace PubSubDemo.Core.Abstractions;

/// <summary>
/// The "transport" side of publish/subscribe: anything that can broadcast a
/// message to whoever is currently listening. A publisher never knows how
/// many subscribers exist, or whether there are any at all.
/// </summary>
public interface IPublisher<in T>
{
    void Publish(T message);
}
