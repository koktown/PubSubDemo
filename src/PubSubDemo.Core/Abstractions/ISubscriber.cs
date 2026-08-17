namespace PubSubDemo.Core.Abstractions;

/// <summary>
/// A consumer that reacts to messages of type <typeparamref name="T"/>.
/// A subscriber knows nothing about where a message came from or who else
/// is listening - it only knows how to handle one when it arrives.
/// </summary>
public interface ISubscriber<in T>
{
    void OnMessage(T message);
}
