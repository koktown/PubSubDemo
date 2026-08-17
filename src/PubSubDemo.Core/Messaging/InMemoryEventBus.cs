using System.Threading;
using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.Messaging;

/// <summary>
/// Minimal in-process message broker - the "transport" that sits between
/// publishers and subscribers so that neither has a direct reference to the
/// other. Swap this implementation out for a real broker (Azure Service Bus,
/// RabbitMQ, Kafka, SignalR...) without changing anything that depends on
/// <see cref="IPublisher{T}"/> / <see cref="ISubscribable{T}"/>.
///
/// Two design choices worth calling out:
///  - Publish takes a snapshot of the subscriber list before iterating, so a
///    subscriber that unsubscribes (or a new one that subscribes) during
///    delivery can't corrupt the in-flight broadcast or deadlock.
///  - A misbehaving subscriber can't take down delivery to the others; its
///    exception is isolated and reported via an optional callback.
/// </summary>
public sealed class InMemoryEventBus<T> : IPublisher<T>, ISubscribable<T>
{
    private readonly object _gate = new();
    private readonly List<ISubscriber<T>> _subscribers = new();
    private readonly Action<ISubscriber<T>, Exception>? _onSubscriberError;

    public InMemoryEventBus(Action<ISubscriber<T>, Exception>? onSubscriberError = null)
    {
        _onSubscriberError = onSubscriberError;
    }

    public int SubscriberCount
    {
        get
        {
            lock (_gate)
            {
                return _subscribers.Count;
            }
        }
    }

    public IDisposable Subscribe(ISubscriber<T> subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        lock (_gate)
        {
            _subscribers.Add(subscriber);
        }

        return new Subscription(() =>
        {
            lock (_gate)
            {
                _subscribers.Remove(subscriber);
            }
        });
    }

    public void Publish(T message)
    {
        ISubscriber<T>[] snapshot;
        lock (_gate)
        {
            snapshot = _subscribers.ToArray();
        }

        foreach (var subscriber in snapshot)
        {
            try
            {
                subscriber.OnMessage(message);
            }
            catch (Exception ex)
            {
                _onSubscriberError?.Invoke(subscriber, ex);
            }
        }
    }

    private sealed class Subscription : IDisposable
    {
        private Action? _unsubscribe;

        public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;

        public void Dispose() => Interlocked.Exchange(ref _unsubscribe, null)?.Invoke();
    }
}
