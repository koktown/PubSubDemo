using PubSubDemo.Core.Messaging;
using PubSubDemo.Core.Tests.TestDoubles;
using Xunit;

namespace PubSubDemo.Core.Tests.Messaging;

public class InMemoryEventBusTests
{
    [Fact]
    public void Publish_DeliversMessageToAllSubscribers()
    {
        var bus = new InMemoryEventBus<string>();
        var subscriberA = new RecordingSubscriber<string>();
        var subscriberB = new RecordingSubscriber<string>();
        bus.Subscribe(subscriberA);
        bus.Subscribe(subscriberB);

        bus.Publish("hello");

        Assert.Equal(new[] { "hello" }, subscriberA.Received);
        Assert.Equal(new[] { "hello" }, subscriberB.Received);
    }

    [Fact]
    public void Publish_WithNoSubscribers_DoesNotThrow()
    {
        var bus = new InMemoryEventBus<string>();

        var exception = Record.Exception(() => bus.Publish("hello"));

        Assert.Null(exception);
    }

    [Fact]
    public void Subscribe_ReturnsHandle_ThatStopsFurtherDeliveryWhenDisposed()
    {
        var bus = new InMemoryEventBus<string>();
        var subscriber = new RecordingSubscriber<string>();
        var subscription = bus.Subscribe(subscriber);

        bus.Publish("first");
        subscription.Dispose();
        bus.Publish("second");

        Assert.Equal(new[] { "first" }, subscriber.Received);
    }

    [Fact]
    public void Publish_WhenOneSubscriberThrows_OtherSubscribersStillReceiveTheMessage()
    {
        var errors = new List<Exception>();
        var bus = new InMemoryEventBus<string>(onSubscriberError: (_, ex) => errors.Add(ex));
        var faulty = new ThrowingSubscriber<string>();
        var healthy = new RecordingSubscriber<string>();
        bus.Subscribe(faulty);
        bus.Subscribe(healthy);

        bus.Publish("hello");

        Assert.Equal(new[] { "hello" }, healthy.Received);
        Assert.Single(errors);
    }

    [Fact]
    public void SubscriberCount_ReflectsActiveSubscriptions()
    {
        var bus = new InMemoryEventBus<string>();
        var subscription = bus.Subscribe(new RecordingSubscriber<string>());

        Assert.Equal(1, bus.SubscriberCount);

        subscription.Dispose();

        Assert.Equal(0, bus.SubscriberCount);
    }
}
